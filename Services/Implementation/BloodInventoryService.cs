using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using Services.Interface;
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

        public BloodInventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedApiResponse<BloodInventoryDto>> GetPagedBloodInventoriesAsync(BloodInventoryParameters parameters)
        {
            try
            {
                var bloodInventoryRepo = _unitOfWork.BloodInventories;
                var (inventories, totalCount) = await bloodInventoryRepo.GetPagedBloodInventoriesAsync(parameters);

                var bloodInventoryDtos = inventories.Select(MapToDto).ToList();

                return new PagedApiResponse<BloodInventoryDto>(bloodInventoryDtos, totalCount, parameters.PageNumber, parameters.PageSize)
                {
                    Message = $"Retrieved {bloodInventoryDtos.Count} blood inventory items out of {totalCount} total"
                };
            }
            catch (Exception ex)
            {
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
                var bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(id);
                
                if (bloodInventory == null)
                    return new ApiResponse<BloodInventoryDto>(HttpStatusCode.NotFound, $"Blood inventory item with ID {id} not found");

                return new ApiResponse<BloodInventoryDto>(MapToDto(bloodInventory));
            }
            catch (Exception ex)
            {
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

                return new ApiResponse<BloodInventoryDto>(MapToDto(bloodInventory), "Blood inventory item created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodInventoryDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodInventoryDto>> UpdateBloodInventoryAsync(int id, UpdateBloodInventoryDto bloodInventoryDto)
        {
            try
            {
                var bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(id);
                
                if (bloodInventory == null)
                    return new ApiResponse<BloodInventoryDto>(HttpStatusCode.NotFound, $"Blood inventory item with ID {id} not found");

                // Update blood inventory
                bloodInventory.QuantityUnits = bloodInventoryDto.QuantityUnits;
                bloodInventory.ExpirationDate = bloodInventoryDto.ExpirationDate;
                bloodInventory.Status = bloodInventoryDto.Status;
                bloodInventory.InventorySource = bloodInventoryDto.InventorySource;

                _unitOfWork.BloodInventories.Update(bloodInventory);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(bloodInventory.Id);

                return new ApiResponse<BloodInventoryDto>(MapToDto(bloodInventory), "Blood inventory item updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<BloodInventoryDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteBloodInventoryAsync(int id)
        {
            try
            {
                var bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(id);
                
                if (bloodInventory == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Blood inventory item with ID {id} not found");

                _unitOfWork.BloodInventories.Delete(bloodInventory);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodInventoryDto>>> GetExpiredInventoryAsync()
        {
            try
            {
                var expiredInventory = await _unitOfWork.BloodInventories.GetExpiredInventoryAsync();
                var expiredInventoryDtos = expiredInventory.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<BloodInventoryDto>>(expiredInventoryDtos)
                {
                    Message = $"Retrieved {expiredInventoryDtos.Count} expired blood inventory items"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<BloodInventoryDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<int>> GetAvailableQuantityAsync(Guid bloodGroupId, Guid componentTypeId)
        {
            try
            {
                var availableQuantity = await _unitOfWork.BloodInventories.GetAvailableQuantityAsync(bloodGroupId, componentTypeId);

                return new ApiResponse<int>(availableQuantity)
                {
                    Message = $"Available quantity: {availableQuantity} units"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<int>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> UpdateInventoryStatusAsync(int id, string newStatus)
        {
            try
            {
                var bloodInventory = await _unitOfWork.BloodInventories.GetByIdWithDetailsAsync(id);
                
                if (bloodInventory == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Blood inventory item with ID {id} not found");

                // Validate status value
                var validStatuses = new[] { "Available", "Reserved", "Used", "Discarded", "Expired" };
                if (!validStatuses.Contains(newStatus))
                {
                    return new ApiResponse(HttpStatusCode.BadRequest, $"Invalid status value. Valid values are: {string.Join(", ", validStatuses)}");
                }

                // Update status
                bloodInventory.Status = newStatus;
                
                _unitOfWork.BloodInventories.Update(bloodInventory);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.OK, $"Blood inventory status updated to '{newStatus}'");
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private BloodInventoryDto MapToDto(BloodInventory bloodInventory)
        {
            string donorName = "";
            if (bloodInventory.DonationEvent?.DonorProfile?.User != null)
            {
                var user = bloodInventory.DonationEvent.DonorProfile.User;
                donorName = $"{user.FirstName} {user.LastName}";
            }

            return new BloodInventoryDto
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
        }
    }
}