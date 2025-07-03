using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Repositories.Interface;
using Services.Interface;
using Shared.Models;
using Shared.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace Services.Implementation
{
    public class BloodRequestService : IBloodRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<BloodRequestService> _logger;
        private readonly IRealTimeNotificationService _realTimeNotificationService;

        public BloodRequestService(IUnitOfWork unitOfWork, ILogger<BloodRequestService> logger = null, IRealTimeNotificationService realTimeNotificationService = null)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _realTimeNotificationService = realTimeNotificationService;
        }

        #region Get Methods

        /// <summary>
        /// Gets all blood requests created by a specific user
        /// </summary>
        /// <param name="userId">ID of the user who created the requests</param>
        /// <param name="onlyActive">Whether to return only active requests</param>
        /// <returns>List of blood requests created by the user</returns>
        public async Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetBloodRequestsByUserIdAsync(Guid userId)
        {
            try
            {
                _logger?.LogInformation("Getting blood requests by user ID: {UserId}", userId);

                // Check if user exists
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger?.LogWarning("User with ID {UserId} not found when getting blood requests", userId);
                    return new ApiResponse<IEnumerable<BloodRequestDto>>(
                        HttpStatusCode.NotFound,
                        $"User with ID {userId} not found");
                }

                // Find blood requests by the specified user
                var bloodRequests = await _unitOfWork.BloodRequests.FindAsync(r =>
                    r.RequestedBy == userId &&
                    r.DeletedTime == null);

                var bloodRequestDtos = bloodRequests
                    .OrderByDescending(r => r.RequestDate)
                    .Select(MapToDto)
                    .ToList();

                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    bloodRequestDtos,
                    $"Found {bloodRequestDtos.Count} blood requests for user {userId}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while getting blood requests by user ID: {UserId}", userId);
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    HttpStatusCode.InternalServerError,
                    $"Error occurred while getting blood requests by user ID: {ex.Message}");
            }
        }

        public async Task<PagedApiResponse<BloodRequestDto>> GetPagedBloodRequestsAsync(BloodRequestParameters parameters)
        {
            try
            {
                _logger?.LogInformation("Getting paged blood requests with parameters: {@Parameters}", parameters);
                
                var bloodRequestRepo = _unitOfWork.BloodRequests;
                var (requests, totalCount) = await bloodRequestRepo.GetPagedBloodRequestsAsync(parameters);

                var bloodRequestDtos = requests.Select(MapToDto).ToList();

                return new PagedApiResponse<BloodRequestDto>(bloodRequestDtos, totalCount, parameters.PageNumber, parameters.PageSize)
                {
                    Message = $"Retrieved {bloodRequestDtos.Count} blood requests out of {totalCount} total"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while getting paged blood requests");
                return new PagedApiResponse<BloodRequestDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<BloodRequestDto>> GetBloodRequestByIdAsync(Guid id)
        {
            try
            {
                _logger?.LogInformation("Getting blood request with ID: {RequestId}", id);
                
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                
                if (bloodRequest == null)
                {
                    _logger?.LogWarning("Blood request with ID {RequestId} not found", id);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.NotFound, $"Blood request with ID {id} not found");
                }

                return new ApiResponse<BloodRequestDto>(MapToDto(bloodRequest));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while getting blood request with ID: {RequestId}", id);
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetActiveBloodRequestsAsync(Guid? bloodGroupId = null)
        {
            try
            {
                _logger?.LogInformation("Getting active blood requests. BloodGroupId filter: {BloodGroupId}", bloodGroupId);
                
                var bloodRequests = await _unitOfWork.BloodRequests.FindAsync(r => 
                    r.IsActive && 
                    (r.Status == "Pending" || r.Status == "Processing") &&
                    r.DeletedTime == null &&
                    (!bloodGroupId.HasValue || r.BloodGroupId == bloodGroupId.Value));
                
                var bloodRequestDtos = bloodRequests.Select(MapToDto).ToList();
                
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    bloodRequestDtos, 
                    $"Found {bloodRequestDtos.Count} active blood requests");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while getting active blood requests");
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    HttpStatusCode.InternalServerError,
                    $"Error occurred while getting active blood requests: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetBloodRequestsByBloodGroupAsync(Guid bloodGroupId, bool onlyActive = true)
        {
            try
            {
                _logger?.LogInformation("Getting blood requests by blood group {BloodGroupId}, OnlyActive: {OnlyActive}", bloodGroupId, onlyActive);
                
                var bloodRequests = await _unitOfWork.BloodRequests.FindAsync(r => 
                    r.BloodGroupId == bloodGroupId &&
                    r.DeletedTime == null &&
                    (!onlyActive || r.IsActive));
                
                var bloodRequestDtos = bloodRequests.Select(MapToDto).ToList();
                
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    bloodRequestDtos, 
                    $"Found {bloodRequestDtos.Count} blood requests for the specified blood group");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while getting blood requests by blood group: {BloodGroupId}", bloodGroupId);
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    HttpStatusCode.InternalServerError,
                    $"Error occurred while getting blood requests by blood group: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetEmergencyBloodRequestsAsync(bool onlyActive = true)
        {
            try
            {
                _logger?.LogInformation("Getting emergency blood requests. OnlyActive: {OnlyActive}", onlyActive);
                
                var requests = await _unitOfWork.BloodRequests.FindAsync(r => 
                    r.IsEmergency && 
                    r.DeletedTime == null &&
                    (!onlyActive || r.IsActive));
                    
                var requestDtos = requests
                    .OrderByDescending(r => r.RequestDate)
                    .Select(MapToDto)
                    .ToList();
                    
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    requestDtos, 
                    $"Found {requestDtos.Count} emergency blood requests");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while getting emergency blood requests");
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    HttpStatusCode.InternalServerError,
                    $"Error occurred while getting emergency blood requests: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetRegularBloodRequestsAsync(bool onlyActive = true)
        {
            try
            {
                _logger?.LogInformation("Getting regular blood requests. OnlyActive: {OnlyActive}", onlyActive);
                
                var requests = await _unitOfWork.BloodRequests.FindAsync(r => 
                    !r.IsEmergency && 
                    r.DeletedTime == null &&
                    (!onlyActive || r.IsActive));
                    
                var requestDtos = requests
                    .OrderByDescending(r => r.RequestDate)
                    .Select(MapToDto)
                    .ToList();
                    
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    requestDtos, 
                    $"Found {requestDtos.Count} regular blood requests");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while getting regular blood requests");
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    HttpStatusCode.InternalServerError,
                    $"Error occurred while getting regular blood requests: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetBloodRequestsByDistanceAsync(
            double latitude, 
            double longitude, 
            double radiusKm, 
            Guid? bloodGroupId = null, 
            string status = null)
        {
            try
            {
                _logger?.LogInformation("Getting blood requests by distance. Lat: {Latitude}, Lon: {Longitude}, Radius: {RadiusKm}km", 
                    latitude, longitude, radiusKm);
                
                var requests = await _unitOfWork.BloodRequests.GetBloodRequestsByDistanceAsync(
                    latitude, longitude, radiusKm, bloodGroupId, status);
                
                var requestDtos = requests.Select(request => MapToDtoWithDistance(request, latitude, longitude)).ToList();
                
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    requestDtos,
                    $"Found {requestDtos.Count} blood requests within {radiusKm}km");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while searching for nearby blood requests");
                return new ApiResponse<IEnumerable<BloodRequestDto>>(
                    HttpStatusCode.InternalServerError,
                    $"Error occurred while searching for nearby blood requests: {ex.Message}");
            }
        }

        public async Task<PagedApiResponse<BloodRequestDto>> GetPagedBloodRequestsByDistanceAsync(
            double latitude, 
            double longitude, 
            double radiusKm, 
            BloodRequestParameters parameters)
        {
            try
            {
                _logger?.LogInformation("Getting paged blood requests by distance. Lat: {Latitude}, Lon: {Longitude}, Radius: {RadiusKm}km", 
                    latitude, longitude, radiusKm);
                
                var (requests, totalCount) = await _unitOfWork.BloodRequests.GetPagedBloodRequestsByDistanceAsync(
                    latitude, longitude, radiusKm, parameters);
                
                var requestDtos = requests.Select(request => MapToDtoWithDistance(request, latitude, longitude)).ToList();
                
                return new PagedApiResponse<BloodRequestDto>(
                    requestDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize)
                {
                    Message = $"Found {requestDtos.Count} blood requests within {radiusKm}km (Page {parameters.PageNumber} of {Math.Ceiling((double)totalCount / parameters.PageSize)})"
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while searching for paged nearby blood requests");
                return new PagedApiResponse<BloodRequestDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = $"Error occurred while searching for nearby blood requests: {ex.Message}"
                };
            }
        }

        #endregion

        #region Create Methods

        public async Task<ApiResponse<BloodRequestDto>> CreateBloodRequestAsync(CreateBloodRequestDto bloodRequestDto)
        {
            try
            {
                _logger?.LogInformation("Creating blood request. IsEmergency: {IsEmergency}", bloodRequestDto.IsEmergency);
                
                // Validate the request
                var validationResult = await ValidateBloodRequestAsync(bloodRequestDto);
                if (validationResult != null)
                    return validationResult;
                    
                // Check for duplicate requests in the last 30 minutes
                if (await IsDuplicateRequestAsync(bloodRequestDto, TimeSpan.FromMinutes(30)))
                {
                    _logger?.LogWarning("Duplicate blood request detected");
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, 
                        "A similar blood request was submitted recently. Please wait before submitting again.");
                }

                // Create blood request
                var bloodRequest = new BloodRequest
                {
                    Id = Guid.NewGuid(),
                    QuantityUnits = bloodRequestDto.QuantityUnits,
                    RequestDate = DateTimeOffset.UtcNow,
                    Status = bloodRequestDto.Status,
                    IsEmergency = bloodRequestDto.IsEmergency,
                    
                    // Regular request fields
                    NeededByDate = bloodRequestDto.NeededByDate,
                    RequestedBy = bloodRequestDto.RequestedBy,
                    
                    // Emergency request fields
                    PatientName = bloodRequestDto.PatientName ?? string.Empty,
                    UrgencyLevel = bloodRequestDto.UrgencyLevel ?? string.Empty,
                    ContactInfo = bloodRequestDto.ContactInfo ?? string.Empty,
                    HospitalName = bloodRequestDto.HospitalName ?? string.Empty,
                    
                    // Blood information
                    BloodGroupId = bloodRequestDto.BloodGroupId,
                    ComponentTypeId = bloodRequestDto.ComponentTypeId,
                    
                    // Location information
                    LocationId = bloodRequestDto.LocationId,
                    Address = bloodRequestDto.Address ?? string.Empty,
                    Latitude = bloodRequestDto.Latitude ?? string.Empty,
                    Longitude = bloodRequestDto.Longitude ?? string.Empty,
                    
                    // Medical notes
                    MedicalNotes = bloodRequestDto.MedicalNotes ?? string.Empty,
                    
                    // Base entity fields
                    CreatedTime = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.BloodRequests.AddAsync(bloodRequest);
                await _unitOfWork.CompleteAsync();

                _logger?.LogInformation("Blood request created successfully. ID: {RequestId}", bloodRequest.Id);
                
                // Reload to get all navigation properties
                bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequest.Id);
                var bloodRequestDto_Result = MapToDto(bloodRequest);

                // Send real-time notifications if service is available
                if (_realTimeNotificationService != null)
                {
                    await _realTimeNotificationService.NotifyStaffOfNewRequest(bloodRequestDto_Result);
                    
                    if (bloodRequest.IsEmergency)
                    {
                        // Create emergency DTO for real-time notifications
                        var emergencyDto = new EmergencyBloodRequestDto
                        {
                            Id = bloodRequest.Id,
                            PatientName = bloodRequest.PatientName,
                            UrgencyLevel = bloodRequest.UrgencyLevel,
                            ContactInfo = bloodRequest.ContactInfo,
                            HospitalName = bloodRequest.HospitalName,
                            QuantityUnits = bloodRequest.QuantityUnits,
                            BloodGroupId = bloodRequest.BloodGroupId,
                            BloodGroupName = bloodRequest.BloodGroup?.GroupName ?? "",
                            ComponentTypeId = bloodRequest.ComponentTypeId,
                            ComponentTypeName = bloodRequest.ComponentType?.Name ?? "",
                            Address = bloodRequest.Address,
                            Latitude = bloodRequest.Latitude,
                            Longitude = bloodRequest.Longitude,
                            MedicalNotes = bloodRequest.MedicalNotes,
                            Status = bloodRequest.Status,
                            RequestDate = bloodRequest.RequestDate,
                            CreatedTime = bloodRequest.CreatedTime
                        };
                        
                        await _realTimeNotificationService.SendEmergencyBloodRequestAlert(emergencyDto);
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }
                    
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                }

                return new ApiResponse<BloodRequestDto>(bloodRequestDto_Result, 
                    bloodRequest.IsEmergency 
                        ? "Emergency blood request created successfully" 
                        : "Blood request created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while creating blood request");
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodRequestDto>> CreatePublicBloodRequestAsync(PublicBloodRequestDto publicRequestDto)
        {
            try
            {
                _logger?.LogInformation("Creating public blood request for patient: {PatientName}", publicRequestDto.PatientName);
                
                // Validate foreign keys exist
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(publicRequestDto.BloodGroupId);
                if (bloodGroup == null)
                {
                    _logger?.LogWarning("Invalid blood group ID: {BloodGroupId}", publicRequestDto.BloodGroupId);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {publicRequestDto.BloodGroupId} does not exist");
                }

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(publicRequestDto.ComponentTypeId);
                if (componentType == null)
                {
                    _logger?.LogWarning("Invalid component type ID: {ComponentTypeId}", publicRequestDto.ComponentTypeId);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {publicRequestDto.ComponentTypeId} does not exist");
                }

                // Validate location exists
                var location = await _unitOfWork.Locations.GetByIdAsync(publicRequestDto.LocationId);
                if (location == null)
                {
                    _logger?.LogWarning("Invalid location ID: {LocationId}", publicRequestDto.LocationId);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Location with ID {publicRequestDto.LocationId} does not exist");
                }

                // Check for duplicate public requests in the last 60 minutes with same contact info
                var recentRequests = await _unitOfWork.BloodRequests.FindAsync(r => 
                    r.IsEmergency &&
                    r.ContactInfo == publicRequestDto.ContactInfo &&
                    r.RequestDate >= DateTimeOffset.UtcNow.AddMinutes(-60));
                    
                if (recentRequests.Any())
                {
                    _logger?.LogWarning("Duplicate public request detected from contact: {ContactInfo}", publicRequestDto.ContactInfo);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, 
                        "A similar emergency request from this contact information was submitted recently. Please wait before submitting again.");
                }
                
                // Create emergency blood request from public request
                var bloodRequest = new BloodRequest
                {
                    Id = Guid.NewGuid(),
                    QuantityUnits = publicRequestDto.QuantityUnits,
                    RequestDate = DateTimeOffset.UtcNow,
                    Status = "Pending",
                    IsEmergency = true, // Public requests are always emergency
                    
                    // Emergency request fields
                    PatientName = publicRequestDto.PatientName,
                    UrgencyLevel = publicRequestDto.UrgencyLevel,
                    ContactInfo = publicRequestDto.ContactInfo,
                    HospitalName = publicRequestDto.HospitalName,
                    
                    // Blood information
                    BloodGroupId = publicRequestDto.BloodGroupId,
                    ComponentTypeId = publicRequestDto.ComponentTypeId,
                    
                    // Location information
                    LocationId = publicRequestDto.LocationId,
                    Address = publicRequestDto.Address,
                    Latitude = publicRequestDto.Latitude,
                    Longitude = publicRequestDto.Longitude,
                    
                    // Medical notes
                    MedicalNotes = publicRequestDto.MedicalNotes ?? string.Empty,
                    
                    // Base entity fields
                    CreatedTime = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.BloodRequests.AddAsync(bloodRequest);
                await _unitOfWork.CompleteAsync();

                _logger?.LogInformation("Public emergency blood request created successfully. ID: {RequestId}", bloodRequest.Id);
                
                // Reload to get all navigation properties
                bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequest.Id);
                var bloodRequestDto_Result = MapToDto(bloodRequest);

                // Send real-time emergency notifications
                if (_realTimeNotificationService != null)
                {
                    var emergencyDto = new EmergencyBloodRequestDto
                    {
                        Id = bloodRequest.Id,
                        PatientName = bloodRequest.PatientName,
                        UrgencyLevel = bloodRequest.UrgencyLevel,
                        ContactInfo = bloodRequest.ContactInfo,
                        HospitalName = bloodRequest.HospitalName,
                        QuantityUnits = bloodRequest.QuantityUnits,
                        BloodGroupId = bloodRequest.BloodGroupId,
                        BloodGroupName = bloodGroup.GroupName,
                        ComponentTypeId = bloodRequest.ComponentTypeId,
                        ComponentTypeName = componentType.Name,
                        Address = bloodRequest.Address,
                        Latitude = bloodRequest.Latitude,
                        Longitude = bloodRequest.Longitude,
                        MedicalNotes = bloodRequest.MedicalNotes,
                        Status = bloodRequest.Status,
                        RequestDate = bloodRequest.RequestDate,
                        CreatedTime = bloodRequest.CreatedTime
                    };
                    
                    await _realTimeNotificationService.SendEmergencyBloodRequestAlert(emergencyDto);
                    await _realTimeNotificationService.NotifyStaffOfNewRequest(bloodRequestDto_Result);
                    await _realTimeNotificationService.UpdateEmergencyDashboard();
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                }

                return new ApiResponse<BloodRequestDto>(bloodRequestDto_Result, "Emergency blood request created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while creating public blood request");
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Checks inventory and fulfills a blood request in a single operation.
        /// This uses the location from the blood request itself and automatically selects inventory.
        /// </summary>
        public async Task<ApiResponse<BloodRequestDto>> FulfillBloodRequestFromInventoryAsync(Guid requestId)
        {
            try
            {
                _logger?.LogInformation("Fulfilling blood request from inventory. RequestId: {RequestId}", requestId);

                // Step 1: Check if the blood request exists
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
                if (bloodRequest == null)
                {
                    _logger?.LogWarning("Blood request with ID {RequestId} not found for fulfillment", requestId);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.NotFound, $"Blood request with ID {requestId} not found");
                }

                // Step 2: Check if the request is already fulfilled
                if (bloodRequest.Status == "Fulfilled")
                {
                    _logger?.LogWarning("Blood request with ID {RequestId} is already fulfilled", requestId);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, "Blood request is already fulfilled");
                }

                // Step 3: Use the location from the blood request itself
                var locationId = bloodRequest.LocationId;

                // Step 4: Check inventory availability
                var availableInventory = await _unitOfWork.BloodInventories.FindAsync(i =>
                    i.BloodGroupId == bloodRequest.BloodGroupId &&
                    i.ComponentTypeId == bloodRequest.ComponentTypeId &&
                    i.Status == "Processing" &&
                    i.ExpirationDate > DateTimeOffset.UtcNow);

                // Calculate total available units
                var totalUnits = availableInventory.Sum(i => i.QuantityUnits);
                var hasEnoughUnits = totalUnits >= bloodRequest.QuantityUnits;

                if (!hasEnoughUnits)
                {
                    _logger?.LogWarning("Insufficient inventory for blood request {RequestId}. Available: {AvailableUnits}, Requested: {RequestedUnits}",
                        requestId, totalUnits, bloodRequest.QuantityUnits);

                    return new ApiResponse<BloodRequestDto>(
                        HttpStatusCode.BadRequest,
                        $"Insufficient inventory. Only {totalUnits} units available for requested {bloodRequest.QuantityUnits} units");
                }

                // Step 5: Create a donation event with status "CompletedFromInventory"
                var donationEvent = new BusinessObjects.Models.DonationEvent
                {
                    Id = Guid.NewGuid(),
                    RequestId = bloodRequest.Id,
                    RequestType = "BloodRequest",
                    BloodGroupId = bloodRequest.BloodGroupId,
                    ComponentTypeId = bloodRequest.ComponentTypeId,
                    LocationId = locationId,
                    Status = "CompletedFromInventory",
                    Notes = "Fulfilled from inventory",
                    CreatedTime = DateTimeOffset.UtcNow,
                    CompletedTime = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                // Step 6: Update inventory items
                int unitsNeeded = bloodRequest.QuantityUnits;
                int unitsAllocated = 0;

                var sortedInventory = availableInventory.OrderBy(i => i.ExpirationDate).ToList(); // Prioritize units expiring sooner

                foreach (var item in sortedInventory)
                {
                    if (unitsAllocated >= unitsNeeded)
                        break;

                    // Track which inventory item was used for the first one
                    if (unitsAllocated == 0)
                    {
                        donationEvent.InventoryId = item.Id;
                    }

                    // Update inventory status
                    item.Status = "Reserved";
                    _unitOfWork.BloodInventories.Update(item);

                    unitsAllocated += item.QuantityUnits;
                }

                // Step 7: Update blood request status
                string previousStatus = bloodRequest.Status;
                bloodRequest.Status = "Fulfilled";
                bloodRequest.LastUpdatedTime = DateTimeOffset.UtcNow;

                _unitOfWork.BloodRequests.Update(bloodRequest);

                // Step 8: Save the donation event
                await _unitOfWork.DonationEvents.AddAsync(donationEvent);

                // Step 9: Commit all changes
                await _unitOfWork.CompleteAsync();

                _logger?.LogInformation("Blood request {RequestId} fulfilled successfully from inventory", bloodRequest.Id);

                // Step 10: Send real-time notifications
                if (_realTimeNotificationService != null)
                {
                    await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                        bloodRequest.Id, "Fulfilled", "Blood request fulfilled from inventory");

                    if (bloodRequest.IsEmergency)
                    {
                        await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                            bloodRequest.Id, "Fulfilled", "Emergency request fulfilled from inventory");
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }

                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                }

                // Step 11: Return the updated blood request
                var result = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequest.Id);
                return new ApiResponse<BloodRequestDto>(MapToDto(result), "Blood request fulfilled successfully from inventory");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while fulfilling blood request from inventory. RequestId: {RequestId}", requestId);
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, $"Error occurred: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BloodRequestDto>> UpdateBloodRequestAsync(Guid id, UpdateBloodRequestDto bloodRequestDto)
        {
            try
            {
                _logger?.LogInformation("Updating blood request with ID: {RequestId}", id);
                
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                
                if (bloodRequest == null)
                {
                    _logger?.LogWarning("Blood request with ID {RequestId} not found for update", id);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.NotFound, $"Blood request with ID {id} not found");
                }

                // Store original status for notification purposes
                string originalStatus = bloodRequest.Status;

                // Validate foreign keys
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodRequestDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {bloodRequestDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(bloodRequestDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {bloodRequestDto.ComponentTypeId} does not exist");

                var location = await _unitOfWork.Locations.GetByIdAsync(bloodRequestDto.LocationId);
                if (location == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Location with ID {bloodRequestDto.LocationId} does not exist");

                // Update blood request
                bloodRequest.QuantityUnits = bloodRequestDto.QuantityUnits;
                bloodRequest.Status = bloodRequestDto.Status;
                bloodRequest.IsEmergency = bloodRequestDto.IsEmergency;
                
                // Regular request fields
                bloodRequest.NeededByDate = bloodRequestDto.NeededByDate;
                
                // Emergency request fields
                bloodRequest.PatientName = bloodRequestDto.PatientName ?? string.Empty;
                bloodRequest.UrgencyLevel = bloodRequestDto.UrgencyLevel ?? string.Empty;
                bloodRequest.ContactInfo = bloodRequestDto.ContactInfo ?? string.Empty;
                bloodRequest.HospitalName = bloodRequestDto.HospitalName ?? string.Empty;
                
                // Blood information
                bloodRequest.BloodGroupId = bloodRequestDto.BloodGroupId;
                bloodRequest.ComponentTypeId = bloodRequestDto.ComponentTypeId;
                
                // Location information
                bloodRequest.LocationId = bloodRequestDto.LocationId;
                bloodRequest.Address = bloodRequestDto.Address ?? string.Empty;
                bloodRequest.Latitude = bloodRequestDto.Latitude ?? string.Empty;
                bloodRequest.Longitude = bloodRequestDto.Longitude ?? string.Empty;
                
                // Medical notes
                bloodRequest.MedicalNotes = bloodRequestDto.MedicalNotes ?? string.Empty;
                bloodRequest.IsActive = bloodRequestDto.IsActive;
                
                // Update timestamp
                bloodRequest.LastUpdatedTime = DateTimeOffset.UtcNow;

                _unitOfWork.BloodRequests.Update(bloodRequest);
                await _unitOfWork.CompleteAsync();

                _logger?.LogInformation("Blood request updated successfully. ID: {RequestId}", id);
                
                // Reload to get all navigation properties
                bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequest.Id);
                var bloodRequestDto_Result = MapToDto(bloodRequest);

                // Send real-time notifications
                if (_realTimeNotificationService != null)
                {
                    await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                        id, bloodRequest.Status, $"Blood request updated from {originalStatus} to {bloodRequest.Status}");
                    
                    if (bloodRequest.IsEmergency)
                    {
                        await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                            id, bloodRequest.Status, $"Emergency request updated to {bloodRequest.Status}");
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }
                    
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                }

                return new ApiResponse<BloodRequestDto>(bloodRequestDto_Result, "Blood request updated successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while updating blood request with ID: {RequestId}", id);
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodRequestDto>> UpdateBloodRequestStatusAsync(Guid id, string status)
        {
            try
            {
                _logger?.LogInformation("Updating blood request status. ID: {RequestId}, New Status: {Status}", id, status);
                
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                
                if (bloodRequest == null)
                {
                    _logger?.LogWarning("Blood request with ID {RequestId} not found for status update", id);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.NotFound, $"Blood request with ID {id} not found");
                }

                // Validate the status value
                var validStatuses = new[] { "Pending", "Processing", "Fulfilled", "Cancelled", "Expired" };
                if (!validStatuses.Contains(status))
                {
                    _logger?.LogWarning("Invalid status value: {Status}", status);
                    return new ApiResponse<BloodRequestDto>(
                        HttpStatusCode.BadRequest, 
                        $"Invalid status value. Valid statuses are: {string.Join(", ", validStatuses)}");
                }

                // Store previous status for notifications or logging
                string previousStatus = bloodRequest.Status;
                
                bloodRequest.Status = status;
                bloodRequest.LastUpdatedTime = DateTimeOffset.UtcNow;

                _unitOfWork.BloodRequests.Update(bloodRequest);
                await _unitOfWork.CompleteAsync();

                _logger?.LogInformation("Blood request status updated from {PreviousStatus} to {NewStatus}. ID: {RequestId}", 
                    previousStatus, status, id);
                
                // Reload to get all navigation properties
                bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequest.Id);
                var bloodRequestDto = MapToDto(bloodRequest);

                // Send real-time notifications
                if (_realTimeNotificationService != null)
                {
                    await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                        id, status, $"Status updated from {previousStatus} to {status}");
                    
                    if (bloodRequest.IsEmergency)
                    {
                        await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                            id, status, $"Emergency request status changed to {status}");
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }
                    
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                }

                return new ApiResponse<BloodRequestDto>(bloodRequestDto, "Blood request status updated successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while updating blood request status. ID: {RequestId}", id);
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<BloodRequestDto>> MarkBloodRequestInactiveAsync(Guid id)
        {
            try
            {
                _logger?.LogInformation("Marking blood request as inactive. ID: {RequestId}", id);
                
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                
                if (bloodRequest == null)
                {
                    _logger?.LogWarning("Blood request with ID {RequestId} not found for marking inactive", id);
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.NotFound, $"Blood request with ID {id} not found");
                }

                bloodRequest.IsActive = false;
                bloodRequest.LastUpdatedTime = DateTimeOffset.UtcNow;

                _unitOfWork.BloodRequests.Update(bloodRequest);
                await _unitOfWork.CompleteAsync();

                _logger?.LogInformation("Blood request marked as inactive successfully. ID: {RequestId}", id);
                
                // Reload to get all navigation properties
                bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequest.Id);
                var bloodRequestDto = MapToDto(bloodRequest);

                // Send real-time notifications
                if (_realTimeNotificationService != null)
                {
                    await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                        id, "Deactivated", "Blood request has been deactivated");
                    
                    if (bloodRequest.IsEmergency)
                    {
                        await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                            id, "Deactivated", "Emergency request has been deactivated");
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }
                    
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                }

                return new ApiResponse<BloodRequestDto>(bloodRequestDto, "Blood request marked as inactive successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while marking blood request inactive. ID: {RequestId}", id);
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region Delete Methods
        
        public async Task<ApiResponse> DeleteBloodRequestAsync(Guid id)
        {
            try
            {
                _logger?.LogInformation("Deleting blood request. ID: {RequestId}", id);
                
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                
                if (bloodRequest == null)
                {
                    _logger?.LogWarning("Blood request with ID {RequestId} not found for deletion", id);
                    return new ApiResponse(HttpStatusCode.NotFound, $"Blood request with ID {id} not found");
                }

                bool wasEmergency = bloodRequest.IsEmergency;

                _unitOfWork.BloodRequests.Delete(bloodRequest);
                await _unitOfWork.CompleteAsync();
                
                _logger?.LogInformation("Blood request deleted successfully. ID: {RequestId}", id);
                
                // Send real-time notifications
                if (_realTimeNotificationService != null)
                {
                    await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                        id, "Deleted", "Blood request has been deleted");
                    
                    if (wasEmergency)
                    {
                        await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                            id, "Deleted", "Emergency request has been deleted");
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }
                    
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                }
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while deleting blood request. ID: {RequestId}", id);
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        #endregion

        #region Helper Methods

        private async Task<ApiResponse<BloodRequestDto>> ValidateBloodRequestAsync(CreateBloodRequestDto dto)
        {
            try
            {
                // Validate blood group
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(dto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {dto.BloodGroupId} does not exist");

                // Validate component type
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(dto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {dto.ComponentTypeId} does not exist");

                // Validate location
                var location = await _unitOfWork.Locations.GetByIdAsync(dto.LocationId);
                if (location == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"Location with ID {dto.LocationId} does not exist");

                // Validate user if provided (for regular requests)
                if (dto.RequestedBy.HasValue)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(dto.RequestedBy.Value);
                    if (user == null)
                        return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, $"User with ID {dto.RequestedBy} does not exist");
                }

                // Validate required fields based on request type
                if (dto.IsEmergency)
                {
                    if (string.IsNullOrEmpty(dto.PatientName))
                        return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, "Patient name is required for emergency requests");
                    
                    if (string.IsNullOrEmpty(dto.ContactInfo))
                        return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, "Contact information is required for emergency requests");
                    
                    if (string.IsNullOrEmpty(dto.UrgencyLevel))
                        return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, "Urgency level is required for emergency requests");
                }
                else
                {
                    if (!dto.RequestedBy.HasValue)
                        return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, "RequestedBy is required for regular blood requests");
                    
                    if (!dto.NeededByDate.HasValue)
                        return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest, "NeededByDate is required for regular blood requests");
                }

                return null; // Validation passed
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred during blood request validation");
                return new ApiResponse<BloodRequestDto>(HttpStatusCode.InternalServerError, "Error occurred during validation: " + ex.Message);
            }
        }

        private async Task<bool> IsDuplicateRequestAsync(CreateBloodRequestDto dto, TimeSpan timeWindow)
        {
            var startTime = DateTimeOffset.UtcNow.Subtract(timeWindow);
            
            // Find similar requests created recently
            var existingRequests = await _unitOfWork.BloodRequests.FindAsync(r => 
                r.BloodGroupId == dto.BloodGroupId &&
                r.ComponentTypeId == dto.ComponentTypeId &&
                r.IsEmergency == dto.IsEmergency &&
                r.RequestDate >= startTime &&
                r.DeletedTime == null);
                
            // For emergency requests, check contact info and patient name
            if (dto.IsEmergency && existingRequests.Any())
            {
                return existingRequests.Any(r => 
                    (!string.IsNullOrEmpty(dto.ContactInfo) && r.ContactInfo == dto.ContactInfo) || 
                    (!string.IsNullOrEmpty(dto.PatientName) && r.PatientName == dto.PatientName));
            }
            
            // For regular requests, check RequestedBy
            if (!dto.IsEmergency && dto.RequestedBy.HasValue && existingRequests.Any())
            {
                return existingRequests.Any(r => r.RequestedBy == dto.RequestedBy);
            }
            
            return false;
        }

        public async Task<ApiResponse<InventoryCheckResultDto>> CheckInventoryForRequestAsync(Guid requestId)
        {
            try
            {
                _logger?.LogInformation("Checking inventory for blood request. ID: {RequestId}", requestId);
                
                var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
                if (request == null)
                {
                    _logger?.LogWarning("Blood request with ID {RequestId} not found for inventory check", requestId);
                    return new ApiResponse<InventoryCheckResultDto>(HttpStatusCode.NotFound, "Blood request not found");
                }
                    
                // Search inventory for matching blood
                var availableInventory = await _unitOfWork.BloodInventories.FindAsync(i => 
                    i.BloodGroupId == request.BloodGroupId && 
                    i.ComponentTypeId == request.ComponentTypeId && 
                    i.Status == "Processing" && 
                    i.ExpirationDate > DateTimeOffset.UtcNow);
                    
                // Calculate total available units
                var totalUnits = availableInventory.Sum(i => i.QuantityUnits);
                var hasEnoughUnits = totalUnits >= request.QuantityUnits;
                
                // Get specific inventory items that could be used
                var inventoryItems = availableInventory
                    .OrderBy(i => i.ExpirationDate) // Prioritize units expiring sooner
                    .Take(Math.Min(5, availableInventory.Count())) // Get up to 5 units to display
                    .Select(i => new InventoryItemDto
                    {
                        Id = i.Id,
                        BloodGroupName = i.BloodGroup?.GroupName ?? "Unknown",
                        ComponentTypeName = i.ComponentType?.Name ?? "Unknown",
                        QuantityUnits = i.QuantityUnits,
                        ExpirationDate = i.ExpirationDate,
                        DaysUntilExpiration = (int)(i.ExpirationDate - DateTimeOffset.UtcNow).TotalDays
                    })
                    .ToList();
                    
                var result = new InventoryCheckResultDto
                {
                    RequestId = requestId,
                    RequestedUnits = request.QuantityUnits,
                    AvailableUnits = totalUnits,
                    HasSufficientInventory = hasEnoughUnits,
                    InventoryItems = inventoryItems
                };
                
                _logger?.LogInformation("Inventory check completed for request {RequestId}. Available: {AvailableUnits}, Requested: {RequestedUnits}", 
                    requestId, totalUnits, request.QuantityUnits);
                
                return new ApiResponse<InventoryCheckResultDto>(
                    result, 
                    hasEnoughUnits 
                        ? $"Found {totalUnits} units available in inventory" 
                        : $"Insufficient inventory. Only {totalUnits} units available for requested {request.QuantityUnits} units");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred while checking inventory for request. ID: {RequestId}", requestId);
                return new ApiResponse<InventoryCheckResultDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private BloodRequestDto MapToDto(BloodRequest bloodRequest)
        {
            return new BloodRequestDto
            {
                Id = bloodRequest.Id,
                QuantityUnits = bloodRequest.QuantityUnits,
                RequestDate = bloodRequest.RequestDate,
                Status = bloodRequest.Status,
                IsEmergency = bloodRequest.IsEmergency,
                
                // Regular request fields
                NeededByDate = bloodRequest.NeededByDate,
                RequestedBy = bloodRequest.RequestedBy,
                RequesterName = bloodRequest.User != null ? $"{bloodRequest.User.FirstName} {bloodRequest.User.LastName}" : "",
                
                // Emergency request fields
                PatientName = bloodRequest.PatientName,
                UrgencyLevel = bloodRequest.UrgencyLevel,
                ContactInfo = bloodRequest.ContactInfo,
                HospitalName = bloodRequest.HospitalName,
                
                // Blood information
                BloodGroupId = bloodRequest.BloodGroupId,
                BloodGroupName = bloodRequest.BloodGroup?.GroupName ?? "",
                ComponentTypeId = bloodRequest.ComponentTypeId,
                ComponentTypeName = bloodRequest.ComponentType?.Name ?? "",
                
                // Location information
                LocationId = bloodRequest.LocationId,
                LocationName = bloodRequest.Location?.Name ?? "",
                Address = bloodRequest.Address,
                Latitude = bloodRequest.Latitude,
                Longitude = bloodRequest.Longitude,
                
                // Medical notes and status
                MedicalNotes = bloodRequest.MedicalNotes,
                IsActive = bloodRequest.IsActive,
                
                // Audit information
                CreatedTime = bloodRequest.CreatedTime,
                LastUpdatedTime = bloodRequest.LastUpdatedTime
            };
        }
        
        private BloodRequestDto MapToDtoWithDistance(BloodRequest bloodRequest, double userLatitude, double userLongitude)
        {
            var dto = MapToDto(bloodRequest);
            
            // Calculate distance if coordinates are available
            if (!string.IsNullOrEmpty(bloodRequest.Latitude) && !string.IsNullOrEmpty(bloodRequest.Longitude))
            {
                if (double.TryParse(bloodRequest.Latitude, out double requestLat) && 
                    double.TryParse(bloodRequest.Longitude, out double requestLon))
                {
                    dto.DistanceKm = GeoCalculator.CalculateDistance(userLatitude, userLongitude, requestLat, requestLon);
                }
            }
            else if (bloodRequest.Location != null && 
                     !string.IsNullOrEmpty(bloodRequest.Location.Latitude) && 
                     !string.IsNullOrEmpty(bloodRequest.Location.Longitude))
            {
                if (double.TryParse(bloodRequest.Location.Latitude, out double locationLat) && 
                    double.TryParse(bloodRequest.Location.Longitude, out double locationLon))
                {
                    dto.DistanceKm = GeoCalculator.CalculateDistance(userLatitude, userLongitude, locationLat, locationLon);
                    dto.Latitude = bloodRequest.Location.Latitude;
                    dto.Longitude = bloodRequest.Location.Longitude;
                }
            }
            
            return dto;
        }
        
        #endregion
    }
}