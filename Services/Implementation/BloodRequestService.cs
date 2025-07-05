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
                    
                    // Fulfillment tracking fields - Set default values
                    IsPickedUp = false,
                    PickupNotes = string.Empty,
                    
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
                    Address = publicRequestDto.Address ?? string.Empty,
                    Latitude = publicRequestDto.Latitude ?? string.Empty,
                    Longitude = publicRequestDto.Longitude ?? string.Empty,
                    
                    // Medical notes
                    MedicalNotes = publicRequestDto.MedicalNotes ?? string.Empty,
                    
                    // Fulfillment tracking fields - Set default values
                    IsPickedUp = false,
                    PickupNotes = string.Empty,
                    
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
        public async Task<ApiResponse<BloodRequestDto>> FulfillBloodRequestFromInventoryAsync(
            Guid requestId,
            FulfillBloodRequestDto fulfillDto)
        {
            try
            {
                _logger?.LogInformation("Fulfilling blood request from inventory. RequestId: {RequestId}", requestId);

                // Step 0: Validate staff exists
                var staff = await _unitOfWork.Users.GetByIdAsync(fulfillDto.StaffId);
                if (staff == null)
                {
                    return new ApiResponse<BloodRequestDto>(
                        HttpStatusCode.BadRequest,
                        $"Staff with ID {fulfillDto.StaffId} not found");
                }

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
                    i.Status == "Available" &&
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

                // Step 5: Update blood request status
                string previousStatus = bloodRequest.Status;
                bloodRequest.Status = "Fulfilled";
                bloodRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                bloodRequest.FulfilledDate = DateTimeOffset.UtcNow;
                bloodRequest.FulfilledByStaffId = fulfillDto.StaffId;

                var vietnamTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                var staffName = $"{staff.FirstName} {staff.LastName}".Trim();

                if (!string.IsNullOrEmpty(fulfillDto.Notes))
                {
                    bloodRequest.MedicalNotes = (bloodRequest.MedicalNotes ?? "") +
                        $"<div class='fulfillment-note'>" +
                        $"<strong>[{vietnamTime:dd/MM/yyyy HH:mm}]</strong> " +
                        $"<span class='action'>Đã đáp ứng yêu cầu</span> bởi <em>{staffName}</em><br/>" +
                        $"<span class='note-content'>{fulfillDto.Notes}</span>" +
                        $"</div>";
                }
                else
                {
                    bloodRequest.MedicalNotes = (bloodRequest.MedicalNotes ?? "") +
                        $"<div class='fulfillment-note'>" +
                        $"<strong>[{vietnamTime:dd/MM/yyyy HH:mm}]</strong> " +
                        $"<span class='action'>Đã đáp ứng yêu cầu</span> bởi <em>{staffName}</em>" +
                        $"</div>";
                }

                _unitOfWork.BloodRequests.Update(bloodRequest);

                // Step 6: Update inventory items
                int unitsNeeded = bloodRequest.QuantityUnits;
                int unitsAllocated = 0;

                var sortedInventory = availableInventory.OrderBy(i => i.ExpirationDate).ToList();

                // Create a tracking log for this fulfillment
                var inventoryUsedInfo = new List<string>();

                foreach (var item in sortedInventory)
                {
                    if (unitsAllocated >= unitsNeeded)
                        break;

                    // Update inventory status and use new fields
                    item.Status = "Used";
                    item.FulfilledRequestId = bloodRequest.Id;
                    item.FulfilledDate = DateTimeOffset.UtcNow;
                    item.FulfillmentNotes = $"Used to fulfill blood request {bloodRequest.Id}";

                    _unitOfWork.BloodInventories.Update(item);

                    // Add to tracking information
                    inventoryUsedInfo.Add($"#{item.Id} ({item.QuantityUnits} đơn vị)");

                    unitsAllocated += item.QuantityUnits;
                }

                // Add detailed inventory usage to request notes
                var inventoryUsageSummary = string.Join(", ", inventoryUsedInfo);
                bloodRequest.MedicalNotes = (bloodRequest.MedicalNotes ?? "") +
                    $"<div class='inventory-info'>" +
                    $"<strong>Kho máu sử dụng:</strong> {inventoryUsageSummary}" +
                    $"</div>";

                // Step 7: Commit all changes
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



        public async Task<ApiResponse<BloodRequestDto>> RecordBloodRequestPickupAsync(
            Guid id, RecordBloodRequestPickupDto pickupDto)
        {
            try
            {
                _logger?.LogInformation("Recording blood request pickup. ID: {RequestId}", id);

                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                if (bloodRequest == null)
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.NotFound,
                        $"Blood request with ID {id} not found");

                // Check if request is fulfilled
                if (bloodRequest.Status != "Fulfilled")
                {
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest,
                        "Only fulfilled blood requests can be picked up");
                }

                // Check if already picked up
                if (bloodRequest.IsPickedUp)
                {
                    return new ApiResponse<BloodRequestDto>(HttpStatusCode.BadRequest,
                        "Blood request has already been picked up");
                }

                // Update blood request pickup status
                bloodRequest.IsPickedUp = true;
                bloodRequest.PickupDate = DateTimeOffset.UtcNow;
                bloodRequest.PickupNotes = pickupDto.PickupNotes ?? string.Empty;
                bloodRequest.LastUpdatedTime = DateTimeOffset.UtcNow;

                _unitOfWork.BloodRequests.Update(bloodRequest);

                // **QUAN TRỌNG**: Cập nhật inventory status khi pickup
                var usedInventoryItems = await _unitOfWork.BloodInventories.FindAsync(i => 
                    i.FulfilledRequestId == id && 
                    i.Status == "Used");

                int dispatchedCount = 0;
                foreach (var inventoryItem in usedInventoryItems)
                {
                    // Chuyển từ "Used" sang "Dispatched" khi pickup
                    inventoryItem.Status = "Dispatched";
                    inventoryItem.FulfillmentNotes = (inventoryItem.FulfillmentNotes ?? "") + 
                        $"\n[{DateTimeOffset.UtcNow}] DISPATCHED: Blood picked up by {pickupDto.RecipientName ?? "recipient"}";
                    
                    _unitOfWork.BloodInventories.Update(inventoryItem);
                    dispatchedCount++;
                }

                var vietnamTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                // Add pickup info to request notes
                bloodRequest.MedicalNotes = (bloodRequest.MedicalNotes ?? "") + 
                    $"<div class='pickup-note'>" +
                    $"<strong>[{vietnamTime:dd/MM/yyyy HH:mm}]</strong> " +
                    $"<span class='action'>Đã lấy máu</span><br/>" +
                    $"<strong>Người nhận:</strong> {pickupDto.RecipientName}<br/>" +
                    $"<strong>Liên hệ:</strong> {pickupDto.RecipientContact}<br/>" +
                    $"<strong>Số đơn vị xuất kho:</strong> {dispatchedCount} đơn vị" +
                    (!string.IsNullOrEmpty(pickupDto.PickupNotes) ? $"<br/><strong>Ghi chú:</strong> {pickupDto.PickupNotes}" : "") +
                    $"</div>";

                await _unitOfWork.CompleteAsync();

                _logger?.LogInformation("Recorded pickup for blood request {RequestId}. {ItemCount} inventory items dispatched", 
                    id, dispatchedCount);

                // Send real-time notifications
                if (_realTimeNotificationService != null)
                {
                    await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                        id, "PickedUp", $"Blood request completed - {dispatchedCount} units dispatched to {pickupDto.RecipientName}");
                    
                    if (bloodRequest.IsEmergency)
                    {
                        await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                            id, "PickedUp", "Emergency blood request completed");
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }
                    
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                    await _realTimeNotificationService.UpdateInventoryDashboard();
                }

                return new ApiResponse<BloodRequestDto>(
                    MapToDto(bloodRequest),
                    $"Pickup recorded successfully - {dispatchedCount} inventory items dispatched to {pickupDto.RecipientName}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error recording pickup for request {RequestId}", id);
                return new ApiResponse<BloodRequestDto>(
                    HttpStatusCode.InternalServerError, ex.Message);
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
                bloodRequest.Status = bloodRequestDto.Status ?? bloodRequest.Status;
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
                var validStatuses = new[] { "Pending", "Processing", "Fulfilled", "Cancelled", "Expired", "Picked Up" };
                if (!validStatuses.Contains(status))
                {
                    _logger?.LogWarning("Invalid status value: {Status}", status);
                    return new ApiResponse<BloodRequestDto>(
                        HttpStatusCode.BadRequest, 
                        $"Invalid status value. Valid statuses are: {string.Join(", ", validStatuses)}");
                }

                // Store previous status for notifications or logging
                string previousStatus = bloodRequest.Status;

                // **XỬ LÝ ĐẶC BIỆT CHO STATUS "Picked Up"**
                if (status == "Picked Up")
                {
                    // Kiểm tra xem request đã được fulfilled chưa
                    if (previousStatus != "Fulfilled")
                    {
                        _logger?.LogWarning("Cannot set status to 'Picked Up' for request {RequestId} with status {CurrentStatus}", id, previousStatus);
                        return new ApiResponse<BloodRequestDto>(
                            HttpStatusCode.BadRequest,
                            "Only fulfilled blood requests can be marked as picked up");
                    }

                    // Kiểm tra xem đã được picked up chưa
                    if (bloodRequest.IsPickedUp)
                    {
                        _logger?.LogWarning("Blood request {RequestId} has already been picked up", id);
                        return new ApiResponse<BloodRequestDto>(
                            HttpStatusCode.BadRequest,
                            "Blood request has already been picked up");
                    }

                    // Thực hiện logic pickup
                    bloodRequest.Status = status;
                    bloodRequest.IsPickedUp = true;
                    bloodRequest.PickupDate = DateTimeOffset.UtcNow;
                    bloodRequest.PickupNotes = "Status updated to Picked Up via status update";
                    bloodRequest.LastUpdatedTime = DateTimeOffset.UtcNow;

                    // **QUAN TRỌNG**: Cập nhật inventory status khi pickup
                    var usedInventoryItems = await _unitOfWork.BloodInventories.FindAsync(i => 
                        i.FulfilledRequestId == id && 
                        i.Status == "Used");

                    int dispatchedCount = 0;
                    foreach (var inventoryItem in usedInventoryItems)
                    {
                        // Chuyển từ "Used" sang "Dispatched" khi pickup
                        inventoryItem.Status = "Dispatched";
                        inventoryItem.FulfillmentNotes = (inventoryItem.FulfillmentNotes ?? "") + 
                            $"\n[{DateTimeOffset.UtcNow}] DISPATCHED: Blood picked up (status updated to Picked Up)";
                        
                        _unitOfWork.BloodInventories.Update(inventoryItem);
                        dispatchedCount++;
                    }

                    var vietnamTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                    // Add pickup info to request notes
                    bloodRequest.MedicalNotes = (bloodRequest.MedicalNotes ?? "") + 
                        $"<div class='pickup-note'>" +
                        $"<strong>[{vietnamTime:dd/MM/yyyy HH:mm}]</strong> " +
                        $"<span class='action'>Đã lấy máu</span> (cập nhật trạng thái)<br/>" +
                        $"<strong>Số đơn vị xuất kho:</strong> {dispatchedCount} đơn vị" +
                        $"</div>";

                    _unitOfWork.BloodRequests.Update(bloodRequest);
                    await _unitOfWork.CompleteAsync();

                    _logger?.LogInformation("Blood request {RequestId} status updated to Picked Up. {ItemCount} inventory items dispatched", 
                        id, dispatchedCount);

                    // Reload to get all navigation properties
                    bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(id);
                    var pickedUpRequestDto = MapToDto(bloodRequest);

                    // Send real-time notifications for pickup
                    if (_realTimeNotificationService != null)
                    {
                        await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                            id, status, $"Blood request completed - {dispatchedCount} units dispatched");
                        
                        if (bloodRequest.IsEmergency)
                        {
                            await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                                id, status, "Emergency blood request completed");
                            await _realTimeNotificationService.UpdateEmergencyDashboard();
                        }
                        
                        await _realTimeNotificationService.UpdateBloodRequestDashboard();
                        await _realTimeNotificationService.UpdateInventoryDashboard();
                    }

                    return new ApiResponse<BloodRequestDto>(pickedUpRequestDto, 
                        $"Blood request status updated to Picked Up successfully - {dispatchedCount} inventory items dispatched");
                }

                // **XỬ LÝ ROLLBACK INVENTORY KHI CANCEL REQUEST ĐÃ FULFILLED**
                if (previousStatus == "Fulfilled" && status == "Cancelled")
                {
                    _logger?.LogInformation("Cancelling fulfilled blood request {RequestId}. Rolling back inventory...", id);
                    
                    // Sử dụng method riêng để rollback inventory
                    int rolledBackCount = await RollbackInventoryAsync(id);
                    
                    var vietnamTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                    
                    // Reset fulfillment tracking fields in blood request
                    bloodRequest.FulfilledDate = null;
                    bloodRequest.FulfilledByStaffId = null;
                    bloodRequest.MedicalNotes = (bloodRequest.MedicalNotes ?? "") + 
                        $"<div class='cancellation-note'>" +
                        $"<strong>[{vietnamTime:dd/MM/yyyy HH:mm}]</strong> " +
                        $"<span class='action'>Đã hủy yêu cầu</span><br/>" +
                        $"<strong>Hoàn trả kho:</strong> {rolledBackCount} đơn vị được khôi phục" +
                        $"</div>";
                    
                    _logger?.LogInformation("Completed inventory rollback for cancelled request {RequestId}. {ItemCount} items restored", 
                        id, rolledBackCount);
                }
                else if (previousStatus == "Fulfilled" && status != "Fulfilled" && status != "Picked Up")
                {
                    // Nếu từ Fulfilled chuyển sang trạng thái khác (không phải Cancelled hoặc Picked Up), cũng cần cảnh báo
                    _logger?.LogWarning("Changing fulfilled request {RequestId} from {PreviousStatus} to {NewStatus}. Consider inventory implications.", 
                        id, previousStatus, status);
                }
                
                // **CẬP NHẬT STATUS THÔNG THƯỜNG**
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
                    string notificationMessage = previousStatus == "Fulfilled" && status == "Cancelled" 
                        ? "Blood request cancelled - inventory has been restored"
                        : $"Status updated from {previousStatus} to {status}";
                        
                    await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                        id, status, notificationMessage);
                    
                    if (bloodRequest.IsEmergency)
                    {
                        await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                            id, status, $"Emergency request status changed to {status}");
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }
                    
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                    
                    // Update inventory dashboard if inventory was affected
                    if (previousStatus == "Fulfilled" && status == "Cancelled")
                    {
                        await _realTimeNotificationService.UpdateInventoryDashboard();
                    }
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

                // **QUAN TRỌNG**: Xử lý rollback inventory nếu request đã fulfilled
                if (bloodRequest.Status == "Fulfilled")
                {
                    _logger?.LogInformation("Deactivating fulfilled blood request {RequestId}. Rolling back inventory...", id);
                    
                    // Rollback inventory khi deactivate fulfilled request
                    int rolledBackCount = await RollbackInventoryAsync(id);
                    
                    // Reset fulfillment tracking fields
                    bloodRequest.FulfilledDate = null;
                    bloodRequest.FulfilledByStaffId = null;
                    bloodRequest.Status = "Cancelled"; // Set to Cancelled when deactivating fulfilled request
                    bloodRequest.MedicalNotes = (bloodRequest.MedicalNotes ?? "") + 
                        $"\n[{DateTimeOffset.UtcNow}] Request deactivated - {rolledBackCount} inventory items restored to available";
                    
                    _logger?.LogInformation("Completed inventory rollback for deactivated request {RequestId}. {ItemCount} items restored", 
                        id, rolledBackCount);
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
                    string notificationMessage = bloodRequest.Status == "Cancelled" 
                        ? "Blood request deactivated - inventory has been restored"
                        : "Blood request has been deactivated";
                        
                    await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                        id, "Deactivated", notificationMessage);
                    
                    if (bloodRequest.IsEmergency)
                    {
                        await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                            id, "Deactivated", "Emergency request has been deactivated");
                        await _realTimeNotificationService.UpdateEmergencyDashboard();
                    }
                    
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                    
                    // Update inventory dashboard if inventory was affected
                    if (bloodRequest.Status == "Cancelled")
                    {
                        await _realTimeNotificationService.UpdateInventoryDashboard();
                    }
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

        /// <summary>
        /// Rollback inventory when a fulfilled blood request is cancelled
        /// </summary>
        /// <param name="requestId">The blood request ID that was cancelled</param>
        /// <returns>Number of inventory items rolled back</returns>
        private async Task<int> RollbackInventoryAsync(Guid requestId)
        {
            try
            {
                _logger?.LogInformation("Starting inventory rollback for cancelled request {RequestId}", requestId);
                
                // Tìm các inventory items đã được sử dụng cho request này (cả "Used" và "Dispatched")
                var usedInventoryItems = await _unitOfWork.BloodInventories.FindAsync(i => 
                    i.FulfilledRequestId == requestId && 
                    (i.Status == "Used" || i.Status == "Dispatched"));
                
                int rolledBackCount = 0;
                int expiredCount = 0;
                int cannotRollbackCount = 0;
                
                var vietnamTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                
                foreach (var inventoryItem in usedInventoryItems)
                {
                    // Kiểm tra xem có thể rollback không
                    if (inventoryItem.Status == "Dispatched")
                    {
                        // Nếu đã được dispatched (picked up), không thể rollback
                        inventoryItem.FulfillmentNotes = (inventoryItem.FulfillmentNotes ?? "") + 
                            $"\n[{vietnamTime:dd/MM/yyyy HH:mm}] ROLLBACK FAILED: Item already dispatched. Request {requestId} was cancelled";
                        
                        _unitOfWork.BloodInventories.Update(inventoryItem);
                        cannotRollbackCount++;
                        
                        _logger?.LogWarning("Cannot rollback dispatched inventory item {InventoryId} for cancelled request {RequestId}", 
                            inventoryItem.Id, requestId);
                    }
                    else if (inventoryItem.ExpirationDate <= DateTimeOffset.UtcNow)
                    {
                        // Nếu đã hết hạn, đánh dấu là Expired
                        inventoryItem.Status = "Expired";
                        inventoryItem.FulfillmentNotes = (inventoryItem.FulfillmentNotes ?? "") + 
                            $"\n[{vietnamTime:dd/MM/yyyy HH:mm}] ROLLBACK FAILED: Item expired before cancellation. Request {requestId} was cancelled";
                        
                        _unitOfWork.BloodInventories.Update(inventoryItem);
                        expiredCount++;
                        
                        _logger?.LogWarning("Cannot rollback expired inventory item {InventoryId} for cancelled request {RequestId}", 
                            inventoryItem.Id, requestId);
                    }
                    else
                    {
                        // Rollback inventory về trạng thái Available (chỉ khi status = "Used" và chưa expired)
                        inventoryItem.Status = "Available";
                        inventoryItem.FulfilledRequestId = null;
                        inventoryItem.FulfilledDate = null;
                        inventoryItem.FulfillmentNotes = (inventoryItem.FulfillmentNotes ?? "") + 
                            $"\n[{vietnamTime:dd/MM/yyyy HH:mm}] ROLLBACK: Request {requestId} was cancelled - item restored to available";
                        
                        _unitOfWork.BloodInventories.Update(inventoryItem);
                        rolledBackCount++;
                        
                        _logger?.LogInformation("Rolled back inventory item {InventoryId} to Available status", inventoryItem.Id);
                    }
                }
                
                // Log tổng kết rollback
                if (cannotRollbackCount > 0 || expiredCount > 0)
                {
                    _logger?.LogWarning("Inventory rollback for request {RequestId}: {RolledBackCount} items restored, {ExpiredCount} items expired, {DispatchedCount} items already dispatched", 
                        requestId, rolledBackCount, expiredCount, cannotRollbackCount);
                }
                else if (rolledBackCount > 0)
                {
                    _logger?.LogInformation("Successfully rolled back {RolledBackCount} inventory items for cancelled request {RequestId}", 
                        rolledBackCount, requestId);
                }
                
                return rolledBackCount;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error occurred during inventory rollback for request {RequestId}", requestId);
                throw; // Re-throw để caller có thể handle
            }
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
                    
                // Search inventory for matching blood - sử dụng status "Available"
                var availableInventory = await _unitOfWork.BloodInventories.FindAsync(i => 
                    i.BloodGroupId == request.BloodGroupId && 
                    i.ComponentTypeId == request.ComponentTypeId && 
                    i.Status == "Available" && // Thay đổi từ "Processing" thành "Available"
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

        /// <summary>
        /// Check if a fulfilled blood request can be safely cancelled (i.e., inventory not picked up yet)
        /// </summary>
        /// <param name="requestId">The blood request ID to check</param>
        /// <returns>Information about cancellation feasibility</returns>
        public async Task<ApiResponse<object>> CheckCancellationFeasibilityAsync(Guid requestId)
        {
            try
            {
                _logger?.LogInformation("Checking cancellation feasibility for request {RequestId}", requestId);
                
                var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
                if (request == null)
                {
                    return new ApiResponse<object>(HttpStatusCode.NotFound, "Blood request not found");
                }
                
                if (request.Status != "Fulfilled")
                {
                    return new ApiResponse<object>(new { 
                        CanCancel = true, 
                        Reason = "Request is not fulfilled yet", 
                        InventoryImpact = "None" 
                    }, "Request can be cancelled without inventory impact");
                }
                
                // Check if blood has been picked up
                if (request.IsPickedUp)
                {
                    return new ApiResponse<object>(new { 
                        CanCancel = false, 
                        Reason = "Blood has already been picked up", 
                        PickupDate = request.PickupDate,
                        InventoryImpact = "Cannot rollback - blood already dispatched to recipient" 
                    }, "Request cannot be cancelled - blood already picked up");
                }
                
                // Check inventory items status and expiration
                var fulfilledInventoryItems = await _unitOfWork.BloodInventories.FindAsync(i => 
                    i.FulfilledRequestId == requestId && 
                    (i.Status == "Used" || i.Status == "Dispatched"));
                
                var usedItems = fulfilledInventoryItems.Where(i => i.Status == "Used").ToList();
                var dispatchedItems = fulfilledInventoryItems.Where(i => i.Status == "Dispatched").ToList();
                var expiredItems = usedItems.Where(i => i.ExpirationDate <= DateTimeOffset.UtcNow).ToList();
                var availableForRollback = usedItems.Where(i => i.ExpirationDate > DateTimeOffset.UtcNow).ToList();
                
                bool canCancelSafely = dispatchedItems.Count == 0; // Chỉ có thể cancel an toàn nếu chưa có items nào được dispatched
                
                return new ApiResponse<object>(new { 
                    CanCancel = canCancelSafely, 
                    Reason = canCancelSafely 
                        ? "Request can be cancelled with inventory rollback" 
                        : "Request cannot be safely cancelled - some inventory items already dispatched",
                    InventoryImpact = new {
                        TotalItems = fulfilledInventoryItems.Count(),
                        UsedItems = usedItems.Count,
                        DispatchedItems = dispatchedItems.Count,
                        CanRollback = availableForRollback.Count,
                        ExpiredItems = expiredItems.Count,
                        RollbackFeasible = availableForRollback.Any() && dispatchedItems.Count == 0
                    }
                }, canCancelSafely 
                    ? "Request can be cancelled with partial or full inventory rollback"
                    : "Request cannot be safely cancelled - inventory already dispatched");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking cancellation feasibility for request {RequestId}", requestId);
                return new ApiResponse<object>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        /// <summary>
        /// Format medical notes for better display with CSS classes
        /// </summary>
        private string FormatMedicalNotesForDisplay(string medicalNotes)
        {
            if (string.IsNullOrWhiteSpace(medicalNotes))
                return "";

            // Nếu đã có HTML formatting thì thêm CSS styles
            if (medicalNotes.Contains("<div class="))
            {
                var styledNotes = medicalNotes
                    .Replace("<div class='fulfillment-note'>", 
                        "<div style='margin: 10px 0; padding: 10px; background: #e8f5e8; border-left: 4px solid #28a745; border-radius: 4px;'>")
                    .Replace("<div class='pickup-note'>", 
                        "<div style='margin: 10px 0; padding: 10px; background: #e6f3ff; border-left: 4px solid #007bff; border-radius: 4px;'>")
                    .Replace("<div class='cancellation-note'>", 
                        "<div style='margin: 10px 0; padding: 10px; background: #ffe6e6; border-left: 4px solid #dc3545; border-radius: 4px;'>")
                    .Replace("<div class='inventory-info'>", 
                        "<div style='margin: 5px 0; padding: 8px; background: #f8f9fa; border: 1px solid #dee2e6; border-radius: 4px; font-size: 0.9em;'>")
                    .Replace("<span class='action'>", 
                        "<span style='color: #495057; font-weight: bold;'>")
                    .Replace("<strong>", 
                        "<strong style='color: #333;'>")
                    .Replace("<em>", 
                        "<em style='color: #6c757d;'>");
                
                return $"<div style='font-family: Arial, sans-serif; line-height: 1.4;'>{styledNotes}</div>";
            }

            // Format các notes cũ thành HTML đẹp hơn
            var formatted = medicalNotes
                .Replace("\n", "<br/>")
                .Replace("[", "<strong style='color: #333;'>[")
                .Replace("]", "]</strong>");

            return $"<div style='font-family: Arial, sans-serif; line-height: 1.4; padding: 10px; background: #f8f9fa; border-radius: 4px;'>{formatted}</div>";
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
                
                // Blood information - ĐẢM BẢO LOAD ĐỦ THÔNG TIN
                BloodGroupId = bloodRequest.BloodGroupId,
                BloodGroupName = bloodRequest.BloodGroup?.GroupName ?? "Chưa xác định",
                ComponentTypeId = bloodRequest.ComponentTypeId,
                ComponentTypeName = bloodRequest.ComponentType?.Name ?? "Chưa xác định",
                
                // Location information
                LocationId = bloodRequest.LocationId,
                LocationName = bloodRequest.Location?.Name ?? "Chưa xác định",
                Address = bloodRequest.Address,
                Latitude = bloodRequest.Latitude,
                Longitude = bloodRequest.Longitude,
                
                // Medical notes and status - ĐỊNH DẠNG HTML ĐẸP HỌN
                MedicalNotes = FormatMedicalNotesForDisplay(bloodRequest.MedicalNotes),
                IsActive = bloodRequest.IsActive,
                
                // Fulfillment tracking
                FulfilledDate = bloodRequest.FulfilledDate,
                FulfilledByStaffId = bloodRequest.FulfilledByStaffId,
                FulfilledByStaffName = "", // Có thể join thêm với User table để lấy tên staff
                IsPickedUp = bloodRequest.IsPickedUp,
                PickupDate = bloodRequest.PickupDate,
                PickupNotes = bloodRequest.PickupNotes,
                
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