using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Services.Interface;
using Services.Interfaces;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class BloodInventoryService : IBloodInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly ILogger<BloodInventoryService> _logger;
        private const int MaxRetries = 3;
        private const string CacheKeyPrefix = "blood_inventory";

        public BloodInventoryService(
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            ILogger<BloodInventoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<PagedApiResponse<BloodInventoryDto>> GetPagedBloodInventoriesAsync(BloodInventoryParameters parameters)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}:paged:page_{parameters.PageNumber}:size_{parameters.PageSize}:group_{parameters.BloodGroupId}:component_{parameters.ComponentTypeId}:status_{parameters.Status}";
                
                var cachedResult = await _cacheService.GetAsync<PagedApiResponse<BloodInventoryDto>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Cache hit for paged blood inventories. Page: {PageNumber}, Size: {PageSize}", 
                        parameters.PageNumber, parameters.PageSize);
                    return cachedResult;
                }

                _logger.LogDebug("Cache miss for paged blood inventories. Fetching from database.");
                var bloodInventoryRepo = _unitOfWork.BloodInventories;
                var (inventories, totalCount) = await bloodInventoryRepo.GetPagedBloodInventoriesAsync(parameters);

                var bloodInventoryDtos = inventories.Select(MapToDto).ToList();

                var result = new PagedApiResponse<BloodInventoryDto>(bloodInventoryDtos, totalCount, parameters.PageNumber, parameters.PageSize)
                {
                    Message = $"Retrieved {bloodInventoryDtos.Count} blood inventory items out of {totalCount} total"
                };

                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Cached paged blood inventories for 5 minutes. Total: {TotalCount}", totalCount);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged blood inventories");
                return new PagedApiResponse<BloodInventoryDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<BloodInventoryDto>> GetBloodInventoryByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}:id:{id}";
                
                var cachedResult = await _cacheService.GetAsync<ApiResponse<BloodInventoryDto>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogDebug("Cache hit for blood inventory ID: {Id}", id);
                    return cachedResult;
                }

                var bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(id);
                
                if (bloodInventory == null)
                {
                    _logger.LogWarning("Blood inventory not found. ID: {Id}", id);
                    return new ApiResponse<BloodInventoryDto>(HttpStatusCode.NotFound, $"Blood inventory item with ID {id} not found");
                }

                var result = new ApiResponse<BloodInventoryDto>(MapToDto(bloodInventory));
                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving blood inventory by ID: {Id}", id);
                return new ApiResponse<BloodInventoryDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodInventoryDto>> CreateBloodInventoryAsync(CreateBloodInventoryDto bloodInventoryDto)
        {
            try
            {
                // Validate foreign keys exist
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodInventoryDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<BloodInventoryDto>(HttpStatusCode.BadRequest, $"Blood group with ID {bloodInventoryDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(bloodInventoryDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<BloodInventoryDto>(HttpStatusCode.BadRequest, $"Component type with ID {bloodInventoryDto.ComponentTypeId} does not exist");

                var donationEvent = await _unitOfWork.DonationEvents.GetByIdAsync(bloodInventoryDto.DonationEventId);
                if (donationEvent == null)
                    return new ApiResponse<BloodInventoryDto>(HttpStatusCode.BadRequest, $"Donation event with ID {bloodInventoryDto.DonationEventId} does not exist");

                // Calculate expiration date if not provided
                if (bloodInventoryDto.ExpirationDate == default)
                {
                    // Calculate based on component type's shelf life
                    bloodInventoryDto.ExpirationDate = DateTimeOffset.UtcNow.AddDays(componentType.ShelfLifeDays);
                }

                // Create blood inventory
                var bloodInventory = new BloodInventory
                {
                    QuantityUnits = bloodInventoryDto.QuantityUnits,
                    ExpirationDate = bloodInventoryDto.ExpirationDate,
                    Status = bloodInventoryDto.Status,
                    InventorySource = bloodInventoryDto.InventorySource,
                    BloodGroupId = bloodInventoryDto.BloodGroupId,
                    ComponentTypeId = bloodInventoryDto.ComponentTypeId,
                    DonationEventId = bloodInventoryDto.DonationEventId
                };

                await _unitOfWork.BloodInventories.AddAsync(bloodInventory);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(bloodInventory.Id);

                var result = new ApiResponse<BloodInventoryDto>(MapToDto(bloodInventory), "Blood inventory item created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };

                await _cacheService.RemoveByPrefixAsync($"{CacheKeyPrefix}:paged");
                await _cacheService.RemoveByPrefixAsync($"{CacheKeyPrefix}:available");

                _logger.LogInformation("Blood inventory created successfully. ID: {Id}, Quantity: {Quantity}, Status: {Status}", 
                    bloodInventory.Id, bloodInventoryDto.QuantityUnits, bloodInventoryDto.Status);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blood inventory");
                return new ApiResponse<BloodInventoryDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodInventoryDto>> UpdateBloodInventoryAsync(int id, UpdateBloodInventoryDto bloodInventoryDto)
        {
            int retryCount = 0;

            while (retryCount < MaxRetries)
            {
                try
                {
                    var bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(id);
                    
                    if (bloodInventory == null)
                    {
                        _logger.LogWarning("Blood inventory not found for update. ID: {Id}", id);
                        return new ApiResponse<BloodInventoryDto>(HttpStatusCode.NotFound, $"Blood inventory item with ID {id} not found");
                    }

                    bloodInventory.QuantityUnits = bloodInventoryDto.QuantityUnits;
                    bloodInventory.ExpirationDate = bloodInventoryDto.ExpirationDate;
                    bloodInventory.Status = bloodInventoryDto.Status;
                    bloodInventory.InventorySource = bloodInventoryDto.InventorySource;

                    _unitOfWork.BloodInventories.Update(bloodInventory);
                    await _unitOfWork.CompleteAsync();

                    await _cacheService.RemoveAsync($"{CacheKeyPrefix}:id:{id}");
                    await _cacheService.RemoveByPrefixAsync($"{CacheKeyPrefix}:paged");
                    await _cacheService.RemoveByPrefixAsync($"{CacheKeyPrefix}:available");

                    _logger.LogInformation("Blood inventory updated successfully. ID: {Id}, Quantity: {Quantity}, Status: {Status}", 
                        id, bloodInventoryDto.QuantityUnits, bloodInventoryDto.Status);

                    return new ApiResponse<BloodInventoryDto>(MapToDto(bloodInventory), "Blood inventory item updated successfully");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "Optimistic locking conflict detected for blood inventory ID: {Id}. Retry {RetryCount}/{MaxRetries}", 
                        id, retryCount, MaxRetries);

                    if (retryCount >= MaxRetries)
                    {
                        _logger.LogError("Max retries reached for blood inventory update. ID: {Id}", id);
                        return new ApiResponse<BloodInventoryDto>(
                            HttpStatusCode.Conflict,
                            "Unable to update blood inventory due to concurrent modification. Please refresh and try again.");
                    }

                    await Task.Delay(100 * retryCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating blood inventory. ID: {Id}", id);
                    return new ApiResponse<BloodInventoryDto>(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return new ApiResponse<BloodInventoryDto>(
                HttpStatusCode.InternalServerError,
                "Failed to update blood inventory after multiple retries");
        }

        public async Task<ApiResponse> DeleteBloodInventoryAsync(int id)
        {
            try
            {
                var bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(id);
                
                if (bloodInventory == null)
                {
                    _logger.LogWarning("Blood inventory not found for deletion. ID: {Id}", id);
                    return new ApiResponse(HttpStatusCode.NotFound, $"Blood inventory item with ID {id} not found");
                }

                _unitOfWork.BloodInventories.Delete(bloodInventory);
                await _unitOfWork.CompleteAsync();
                
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}:id:{id}");
                await _cacheService.RemoveByPrefixAsync($"{CacheKeyPrefix}:paged");
                await _cacheService.RemoveByPrefixAsync($"{CacheKeyPrefix}:available");

                _logger.LogInformation("Blood inventory deleted successfully. ID: {Id}", id);
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blood inventory. ID: {Id}", id);
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodInventoryDto>>> GetExpiredInventoryAsync()
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}:expired";
                
                var cachedResult = await _cacheService.GetAsync<ApiResponse<IEnumerable<BloodInventoryDto>>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogDebug("Cache hit for expired blood inventories");
                    return cachedResult;
                }

                var expiredInventory = await _unitOfWork.BloodInventories.GetExpiredInventoryAsync();
                var expiredInventoryDtos = expiredInventory.Select(MapToDto).ToList();

                var result = new ApiResponse<IEnumerable<BloodInventoryDto>>(expiredInventoryDtos)
                {
                    Message = $"Retrieved {expiredInventoryDtos.Count} expired blood inventory items"
                };

                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
                _logger.LogInformation("Expired blood inventories cached for 5 minutes. Total: {TotalCount}", expiredInventoryDtos.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving expired blood inventories");
                return new ApiResponse<IEnumerable<BloodInventoryDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<int>> GetAvailableQuantityAsync(Guid bloodGroupId, Guid componentTypeId)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}:available:group_{bloodGroupId}:component_{componentTypeId}";
                
                var cachedResult = await _cacheService.GetAsync<ApiResponse<int>>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogDebug("Cache hit for available quantity. BloodGroup: {BloodGroupId}, Component: {ComponentTypeId}", 
                        bloodGroupId, componentTypeId);
                    return cachedResult;
                }

                var availableQuantity = await _unitOfWork.BloodInventories.GetAvailableQuantityAsync(bloodGroupId, componentTypeId);

                var result = new ApiResponse<int>(availableQuantity)
                {
                    Message = $"Available quantity: {availableQuantity} units"
                };

                await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(3));
                _logger.LogInformation("Available quantity cached. BloodGroup: {BloodGroupId}, Component: {ComponentTypeId}, Quantity: {Quantity}", 
                    bloodGroupId, componentTypeId, availableQuantity);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available quantity. BloodGroup: {BloodGroupId}, Component: {ComponentTypeId}", 
                    bloodGroupId, componentTypeId);
                return new ApiResponse<int>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateInventoryStatusAsync(int id, string newStatus)
        {
            int retryCount = 0;

            while (retryCount < MaxRetries)
            {
                try
                {
                    var bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(id);
                    
                    if (bloodInventory == null)
                    {
                        _logger.LogWarning("Blood inventory not found for status update. ID: {Id}", id);
                        return new ApiResponse(HttpStatusCode.NotFound, $"Blood inventory item with ID {id} not found");
                    }

                    var validStatuses = new[] { "Available", "Reserved", "Used", "Discarded", "Expired" };
                    if (!validStatuses.Contains(newStatus))
                    {
                        return new ApiResponse(HttpStatusCode.BadRequest, $"Invalid status value. Valid values are: {string.Join(", ", validStatuses)}");
                    }

                    var oldStatus = bloodInventory.Status;
                    bloodInventory.Status = newStatus;
                    
                    _unitOfWork.BloodInventories.Update(bloodInventory);
                    await _unitOfWork.CompleteAsync();

                    await _cacheService.RemoveAsync($"{CacheKeyPrefix}:id:{id}");
                    await _cacheService.RemoveByPrefixAsync($"{CacheKeyPrefix}:paged");
                    await _cacheService.RemoveByPrefixAsync($"{CacheKeyPrefix}:available");

                    _logger.LogInformation("Blood inventory status updated. ID: {Id}, OldStatus: {OldStatus}, NewStatus: {NewStatus}", 
                        id, oldStatus, newStatus);
                    
                    return new ApiResponse(HttpStatusCode.OK, $"Blood inventory status updated to '{newStatus}'");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "Optimistic locking conflict for status update. ID: {Id}. Retry {RetryCount}/{MaxRetries}", 
                        id, retryCount, MaxRetries);

                    if (retryCount >= MaxRetries)
                    {
                        return new ApiResponse(
                            HttpStatusCode.Conflict,
                            "Unable to update status due to concurrent modification. Please refresh and try again.");
                    }

                    await Task.Delay(100 * retryCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating blood inventory status. ID: {Id}", id);
                    return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return new ApiResponse(
                HttpStatusCode.InternalServerError,
                "Failed to update status after multiple retries");
        }

        private BloodInventoryDto MapToDto(BloodInventory bloodInventory)
        {
            string donorName = "";
            if (bloodInventory.DonationEvent?.DonorProfile?.User != null)
            {
                var user = bloodInventory.DonationEvent.DonorProfile.User;
                donorName = $"{user.FirstName} {user.LastName}";
            }

            // Create the main DTO
            var inventoryDto = new BloodInventoryDto
            {
                Id = bloodInventory.Id,
                QuantityUnits = bloodInventory.QuantityUnits,
                ExpirationDate = bloodInventory.ExpirationDate,
                Status = bloodInventory.Status,
                InventorySource = bloodInventory.InventorySource,
                BloodGroupId = bloodInventory.BloodGroupId,
                BloodGroupName = bloodInventory.BloodGroup?.GroupName ?? "",
                ComponentTypeId = bloodInventory.ComponentTypeId,
                ComponentTypeName = bloodInventory.ComponentType?.Name ?? "",
                DonationEventId = bloodInventory.DonationEventId,
                DonorName = donorName
            };

            // Add detailed donation event information if available
            if (bloodInventory.DonationEvent != null)
            {
                var donationEvent = bloodInventory.DonationEvent;
                var donorProfile = donationEvent.DonorProfile;
                var user = donorProfile?.User;
                
                inventoryDto.DonationEvent = new DonationEventInfoDto
                {
                    Id = donationEvent.Id,
                    DonorId = donationEvent.DonorId,
                    DonorName = user != null ? $"{user.FirstName} {user.LastName}" : "",
                    DonorPhone = user?.PhoneNumber ?? "",
                    DonationDate = donationEvent.DonationDate,
                    QuantityDonated = donationEvent.QuantityDonated,
                    QuantityUnits = donationEvent.QuantityUnits,
                    IsUsable = donationEvent.IsUsable,
                    LocationId = donationEvent.LocationId,
                    LocationName = donationEvent.Location?.Name ?? "",
                    CreatedTime = donationEvent.CreatedTime,
                    CompletedTime = donationEvent.CompletedTime
                };
            }

            return inventoryDto;
        }
    }
}