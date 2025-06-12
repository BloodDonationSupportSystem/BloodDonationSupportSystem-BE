using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.Extensions.Configuration;
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
    public class EmergencyRequestService : IEmergencyRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IBloodCompatibilityService _bloodCompatibilityService;

        public EmergencyRequestService(
            IUnitOfWork unitOfWork, 
            INotificationService notificationService,
            IEmailService emailService,
            IConfiguration configuration,
            IBloodCompatibilityService bloodCompatibilityService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _emailService = emailService;
            _configuration = configuration;
            _bloodCompatibilityService = bloodCompatibilityService;
        }

        public async Task<PagedApiResponse<EmergencyRequestDto>> GetPagedEmergencyRequestsAsync(EmergencyRequestParameters parameters)
        {
            try
            {
                var emergencyRequestRepo = _unitOfWork.EmergencyRequests;
                var (requests, totalCount) = await emergencyRequestRepo.GetPagedEmergencyRequestsAsync(parameters);

                var emergencyRequestDtos = requests.Select(MapToDto).ToList();

                return new PagedApiResponse<EmergencyRequestDto>(emergencyRequestDtos, totalCount, parameters.PageNumber, parameters.PageSize)
                {
                    Message = $"Retrieved {emergencyRequestDtos.Count} emergency requests out of {totalCount} total"
                };
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<EmergencyRequestDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<EmergencyRequestDto>> GetEmergencyRequestByIdAsync(Guid id)
        {
            try
            {
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(id);
                
                if (emergencyRequest == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.NotFound, $"Emergency request with ID {id} not found");

                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest));
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<EmergencyRequestDto>> CreateEmergencyRequestAsync(CreateEmergencyRequestDto emergencyRequestDto)
        {
            try
            {
                // Validate foreign keys exist
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(emergencyRequestDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {emergencyRequestDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(emergencyRequestDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {emergencyRequestDto.ComponentTypeId} does not exist");

                // Validate location if provided
                if (emergencyRequestDto.LocationId.HasValue)
                {
                    var location = await _unitOfWork.Locations.GetByIdAsync(emergencyRequestDto.LocationId.Value);
                    if (location == null)
                        return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Location with ID {emergencyRequestDto.LocationId} does not exist");
                }

                // Create emergency request
                var emergencyRequest = new EmergencyRequest
                {
                    PatientName = emergencyRequestDto.PatientName,
                    ContactInfo = emergencyRequestDto.ContactInfo,
                    QuantityUnits = emergencyRequestDto.QuantityUnits,
                    Status = emergencyRequestDto.Status,
                    UrgencyLevel = emergencyRequestDto.UrgencyLevel,
                    RequestDate = DateTimeOffset.UtcNow,
                    BloodGroupId = emergencyRequestDto.BloodGroupId,
                    ComponentTypeId = emergencyRequestDto.ComponentTypeId,
                    CreatedTime = DateTimeOffset.UtcNow,
                    CreatedBy = "System", // In a real application, this would be the user's name
                    
                    // Add location information
                    LocationId = emergencyRequestDto.LocationId,
                    Address = emergencyRequestDto.Address,
                    Latitude = emergencyRequestDto.Latitude,
                    Longitude = emergencyRequestDto.Longitude,
                    
                    // Add hospital information
                    HospitalName = emergencyRequestDto.HospitalName,
                    
                    // Add medical notes
                    MedicalNotes = emergencyRequestDto.MedicalNotes,
                    
                    // Default to active
                    IsActive = true
                };

                await _unitOfWork.EmergencyRequests.AddAsync(emergencyRequest);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(emergencyRequest.Id);
                
                // Send notifications to potential donors
                await SendNotificationsForEmergencyRequest(emergencyRequest);

                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest), "Emergency request created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<EmergencyRequestDto>> UpdateEmergencyRequestAsync(Guid id, UpdateEmergencyRequestDto emergencyRequestDto)
        {
            try
            {
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(id);
                
                if (emergencyRequest == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.NotFound, $"Emergency request with ID {id} not found");

                // Validate foreign keys
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(emergencyRequestDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {emergencyRequestDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(emergencyRequestDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {emergencyRequestDto.ComponentTypeId} does not exist");

                // Validate location if provided
                if (emergencyRequestDto.LocationId.HasValue)
                {
                    var location = await _unitOfWork.Locations.GetByIdAsync(emergencyRequestDto.LocationId.Value);
                    if (location == null)
                        return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Location with ID {emergencyRequestDto.LocationId} does not exist");
                }

                // Update emergency request
                emergencyRequest.PatientName = emergencyRequestDto.PatientName;
                emergencyRequest.ContactInfo = emergencyRequestDto.ContactInfo;
                emergencyRequest.QuantityUnits = emergencyRequestDto.QuantityUnits;
                emergencyRequest.Status = emergencyRequestDto.Status;
                emergencyRequest.UrgencyLevel = emergencyRequestDto.UrgencyLevel;
                emergencyRequest.BloodGroupId = emergencyRequestDto.BloodGroupId;
                emergencyRequest.ComponentTypeId = emergencyRequestDto.ComponentTypeId;
                emergencyRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                emergencyRequest.LastUpdatedBy = "System"; // In a real application, this would be the user's name
                
                // Update location information
                emergencyRequest.LocationId = emergencyRequestDto.LocationId;
                emergencyRequest.Address = emergencyRequestDto.Address;
                emergencyRequest.Latitude = emergencyRequestDto.Latitude;
                emergencyRequest.Longitude = emergencyRequestDto.Longitude;
                
                // Update hospital information
                emergencyRequest.HospitalName = emergencyRequestDto.HospitalName;
                
                // Update medical notes
                emergencyRequest.MedicalNotes = emergencyRequestDto.MedicalNotes;
                
                // Update active status
                emergencyRequest.IsActive = emergencyRequestDto.IsActive;

                _unitOfWork.EmergencyRequests.Update(emergencyRequest);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(emergencyRequest.Id);
                
                // If the status or urgency changed, send notifications
                if (emergencyRequest.IsActive && (emergencyRequest.Status == "Active" || emergencyRequest.UrgencyLevel == "Critical"))
                {
                    await SendNotificationsForEmergencyRequest(emergencyRequest);
                }

                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest), "Emergency request updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteEmergencyRequestAsync(Guid id)
        {
            try
            {
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(id);
                
                if (emergencyRequest == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Emergency request with ID {id} not found");

                // Set deleted time instead of actually deleting
                emergencyRequest.DeletedTime = DateTimeOffset.UtcNow;
                emergencyRequest.IsActive = false;
                _unitOfWork.EmergencyRequests.Update(emergencyRequest);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        public async Task<ApiResponse<IEnumerable<EmergencyRequestDto>>> GetEmergencyRequestsByDistanceAsync(
            double latitude, 
            double longitude, 
            double radiusKm, 
            Guid? bloodGroupId = null, 
            string urgencyLevel = null,
            bool? isActive = null)
        {
            try
            {
                var requests = await _unitOfWork.EmergencyRequests.GetEmergencyRequestsByDistanceAsync(
                    latitude, longitude, radiusKm, bloodGroupId, urgencyLevel, isActive);
                
                var requestDtos = requests.Select(r => 
                {
                    var dto = MapToDto(r);
                    
                    // Calculate and set distance
                    double lat2 = 0, lon2 = 0;
                    
                    if (!string.IsNullOrEmpty(r.Latitude) && !string.IsNullOrEmpty(r.Longitude) &&
                        double.TryParse(r.Latitude, out lat2) && double.TryParse(r.Longitude, out lon2))
                    {
                        dto.DistanceKm = Shared.Utilities.GeoCalculator.CalculateDistance(latitude, longitude, lat2, lon2);
                    }
                    else if (r.Location != null && !string.IsNullOrEmpty(r.Location.Latitude) && !string.IsNullOrEmpty(r.Location.Longitude) &&
                             double.TryParse(r.Location.Latitude, out lat2) && double.TryParse(r.Location.Longitude, out lon2))
                    {
                        dto.DistanceKm = Shared.Utilities.GeoCalculator.CalculateDistance(latitude, longitude, lat2, lon2);
                    }
                    
                    return dto;
                }).ToList();
                
                return new ApiResponse<IEnumerable<EmergencyRequestDto>>(requestDtos,
                    $"Found {requestDtos.Count} emergency requests within {radiusKm} km radius");
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<EmergencyRequestDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PagedApiResponse<EmergencyRequestDto>> GetPagedEmergencyRequestsByDistanceAsync(
            double latitude, 
            double longitude, 
            double radiusKm, 
            EmergencyRequestParameters parameters)
        {
            try
            {
                var (requests, totalCount) = await _unitOfWork.EmergencyRequests.GetPagedEmergencyRequestsByDistanceAsync(
                    latitude, longitude, radiusKm, parameters);
                
                var requestDtos = requests.Select(r => 
                {
                    var dto = MapToDto(r);
                    
                    // Calculate and set distance
                    double lat2 = 0, lon2 = 0;
                    
                    if (!string.IsNullOrEmpty(r.Latitude) && !string.IsNullOrEmpty(r.Longitude) &&
                        double.TryParse(r.Latitude, out lat2) && double.TryParse(r.Longitude, out lon2))
                    {
                        dto.DistanceKm = Shared.Utilities.GeoCalculator.CalculateDistance(latitude, longitude, lat2, lon2);
                    }
                    else if (r.Location != null && !string.IsNullOrEmpty(r.Location.Latitude) && !string.IsNullOrEmpty(r.Location.Longitude) &&
                             double.TryParse(r.Location.Latitude, out lat2) && double.TryParse(r.Location.Longitude, out lon2))
                    {
                        dto.DistanceKm = Shared.Utilities.GeoCalculator.CalculateDistance(latitude, longitude, lat2, lon2);
                    }
                    
                    return dto;
                }).ToList();
                
                return new PagedApiResponse<EmergencyRequestDto>(
                    requestDtos, 
                    totalCount, 
                    parameters.PageNumber, 
                    parameters.PageSize)
                {
                    Message = $"Found {requestDtos.Count} emergency requests within {radiusKm} km radius (page {parameters.PageNumber} of {Math.Ceiling((double)totalCount / parameters.PageSize)})"
                };
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<EmergencyRequestDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }
        
        public async Task<ApiResponse<EmergencyRequestDto>> CreatePublicEmergencyRequestAsync(PublicEmergencyRequestDto publicRequestDto)
        {
            try
            {
                // Validate CAPTCHA
                if (!ValidateCaptcha(publicRequestDto.CaptchaToken))
                {
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, "CAPTCHA validation failed");
                }
                
                // Validate foreign keys exist
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(publicRequestDto.BloodGroupId);
                if (bloodGroup == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Blood group with ID {publicRequestDto.BloodGroupId} does not exist");

                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(publicRequestDto.ComponentTypeId);
                if (componentType == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Component type with ID {publicRequestDto.ComponentTypeId} does not exist");

                // Create emergency request with default "Pending" status for moderation
                var emergencyRequest = new EmergencyRequest
                {
                    PatientName = publicRequestDto.PatientName,
                    ContactInfo = publicRequestDto.ContactInfo,
                    QuantityUnits = publicRequestDto.QuantityUnits,
                    Status = "Pending", // Default status for public requests
                    UrgencyLevel = "High", // Default urgency for public requests
                    RequestDate = DateTimeOffset.UtcNow,
                    BloodGroupId = publicRequestDto.BloodGroupId,
                    ComponentTypeId = publicRequestDto.ComponentTypeId,
                    CreatedTime = DateTimeOffset.UtcNow,
                    CreatedBy = "Public", // Indicate this was created by a public user
                    
                    // Add location information
                    Address = publicRequestDto.Address,
                    Latitude = publicRequestDto.Latitude,
                    Longitude = publicRequestDto.Longitude,
                    
                    // Add hospital information
                    HospitalName = publicRequestDto.HospitalName,
                    
                    // Add medical notes
                    MedicalNotes = publicRequestDto.MedicalNotes,
                    
                    // Set as active but pending approval
                    IsActive = true
                };

                await _unitOfWork.EmergencyRequests.AddAsync(emergencyRequest);
                await _unitOfWork.CompleteAsync();

                // Reload to get all navigation properties
                emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(emergencyRequest.Id);
                
                // Send notification to admins for approval
                await NotifyAdminsAboutNewEmergencyRequest(emergencyRequest);

                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest), 
                    "Emergency request submitted successfully and is pending approval")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        public async Task<ApiResponse<IEnumerable<EmergencyRequestDto>>> GetActiveEmergencyRequestsAsync(Guid? bloodGroupId = null)
        {
            try
            {
                var requests = await _unitOfWork.EmergencyRequests.GetActiveEmergencyRequestsAsync(bloodGroupId);
                var requestDtos = requests.Select(MapToDto).ToList();
                
                return new ApiResponse<IEnumerable<EmergencyRequestDto>>(requestDtos,
                    $"Found {requestDtos.Count} active emergency requests");
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<EmergencyRequestDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        public async Task<ApiResponse<IEnumerable<EmergencyRequestDto>>> GetEmergencyRequestsByBloodGroupAsync(Guid bloodGroupId, bool onlyActive = true)
        {
            try
            {
                var requests = await _unitOfWork.EmergencyRequests.GetEmergencyRequestsByBloodGroupAsync(bloodGroupId, onlyActive);
                var requestDtos = requests.Select(MapToDto).ToList();
                
                return new ApiResponse<IEnumerable<EmergencyRequestDto>>(requestDtos,
                    $"Found {requestDtos.Count} emergency requests for blood group ID {bloodGroupId}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<EmergencyRequestDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        public async Task<ApiResponse<EmergencyRequestDto>> UpdateEmergencyRequestStatusAsync(Guid id, string status)
        {
            try
            {
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(id);
                
                if (emergencyRequest == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.NotFound, $"Emergency request with ID {id} not found");
                
                // Validate status
                var validStatuses = new[] { "Pending", "Active", "Fulfilled", "Cancelled" };
                if (!validStatuses.Contains(status))
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.BadRequest, $"Invalid status '{status}'. Valid statuses are: {string.Join(", ", validStatuses)}");
                
                // Update status
                emergencyRequest.Status = status;
                
                // If fulfilled or cancelled, mark as inactive
                if (status == "Fulfilled" || status == "Cancelled")
                {
                    emergencyRequest.IsActive = false;
                }
                else if (status == "Active")
                {
                    emergencyRequest.IsActive = true;
                }
                
                emergencyRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                emergencyRequest.LastUpdatedBy = "System"; // In a real application, this would be the user's name
                
                _unitOfWork.EmergencyRequests.Update(emergencyRequest);
                await _unitOfWork.CompleteAsync();
                
                // Reload to get all navigation properties
                emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(emergencyRequest.Id);
                
                // If status changed to Active, send notifications
                if (status == "Active")
                {
                    await SendNotificationsForEmergencyRequest(emergencyRequest);
                }
                
                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest), 
                    $"Emergency request status updated to '{status}' successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        public async Task<ApiResponse<EmergencyRequestDto>> MarkEmergencyRequestInactiveAsync(Guid id)
        {
            try
            {
                var emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdAsync(id);
                
                if (emergencyRequest == null)
                    return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.NotFound, $"Emergency request with ID {id} not found");
                
                // Mark as inactive
                emergencyRequest.IsActive = false;
                
                // If status is not already Fulfilled or Cancelled, set to Fulfilled
                if (emergencyRequest.Status != "Fulfilled" && emergencyRequest.Status != "Cancelled")
                {
                    emergencyRequest.Status = "Fulfilled";
                }
                
                emergencyRequest.LastUpdatedTime = DateTimeOffset.UtcNow;
                emergencyRequest.LastUpdatedBy = "System"; // In a real application, this would be the user's name
                
                _unitOfWork.EmergencyRequests.Update(emergencyRequest);
                await _unitOfWork.CompleteAsync();
                
                // Reload to get all navigation properties
                emergencyRequest = await _unitOfWork.EmergencyRequests.GetByIdWithDetailsAsync(emergencyRequest.Id);
                
                return new ApiResponse<EmergencyRequestDto>(MapToDto(emergencyRequest), 
                    "Emergency request marked as inactive successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<EmergencyRequestDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private EmergencyRequestDto MapToDto(EmergencyRequest emergencyRequest)
        {
            return new EmergencyRequestDto
            {
                Id = emergencyRequest.Id,
                PatientName = emergencyRequest.PatientName,
                ContactInfo = emergencyRequest.ContactInfo,
                QuantityUnits = emergencyRequest.QuantityUnits,
                Status = emergencyRequest.Status,
                UrgencyLevel = emergencyRequest.UrgencyLevel,
                RequestDate = emergencyRequest.RequestDate,
                CreatedTime = emergencyRequest.CreatedTime,
                LastUpdatedTime = emergencyRequest.LastUpdatedTime,
                
                BloodGroupId = emergencyRequest.BloodGroupId,
                BloodGroupName = emergencyRequest.BloodGroup?.GroupName ?? "",
                
                ComponentTypeId = emergencyRequest.ComponentTypeId,
                ComponentTypeName = emergencyRequest.ComponentType?.Name ?? "",
                
                // Location information
                LocationId = emergencyRequest.LocationId,
                LocationName = emergencyRequest.Location?.Name ?? "",
                Address = emergencyRequest.Address ?? "",
                Latitude = emergencyRequest.Latitude ?? "",
                Longitude = emergencyRequest.Longitude ?? "",
                
                // Hospital information
                HospitalName = emergencyRequest.HospitalName ?? "",
                
                // Medical notes
                MedicalNotes = emergencyRequest.MedicalNotes ?? "",
                
                // Active status
                IsActive = emergencyRequest.IsActive
            };
        }
        
        // Helper methods
        private bool ValidateCaptcha(string captchaToken)
        {
            // In a real application, this would validate the CAPTCHA token with a service like reCAPTCHA
            // For this example, we'll just return true
            return true;
        }
        
        private async Task SendNotificationsForEmergencyRequest(EmergencyRequest emergencyRequest)
        {
            try
            {
                // Get compatible blood groups
                var compatibleBloodGroups = await _bloodCompatibilityService.GetCompatibleDonorBloodGroupsAsync(emergencyRequest.BloodGroupId);
                
                if (!compatibleBloodGroups.Success || compatibleBloodGroups.Data == null)
                {
                    // If we can't get compatible blood groups, use just the exact blood group
                    compatibleBloodGroups = new ApiResponse<IEnumerable<Guid>>(new List<Guid> { emergencyRequest.BloodGroupId });
                }
                
                // Get donors with compatible blood groups
                var potentialDonors = new List<User>();
                foreach (var bloodGroupId in compatibleBloodGroups.Data)
                {
                    var donors = await _unitOfWork.DonorProfiles.GetDonorsByBloodGroupAsync(bloodGroupId, true);
                    foreach (var donor in donors)
                    {
                        if (donor.User != null && !potentialDonors.Any(d => d.Id == donor.User.Id))
                        {
                            potentialDonors.Add(donor.User);
                        }
                    }
                }
                
                // Send notifications to potential donors
                foreach (var donor in potentialDonors)
                {
                    var notification = new Notification
                    {
                        UserId = donor.Id,
                        Title = "Emergency Blood Request",
                        Message = $"Urgent need for {emergencyRequest.BloodGroup?.GroupName} blood at {emergencyRequest.HospitalName}. " +
                                  $"Patient: {emergencyRequest.PatientName}. Please respond if you can help.",
                        Type = "EmergencyRequest",
                        ReferenceId = emergencyRequest.Id.ToString(),
                        IsRead = false,
                        CreatedTime = DateTimeOffset.UtcNow
                    };
                    
                    await _unitOfWork.Notifications.AddAsync(notification);
                    
                    // Send email if user has email
                    if (!string.IsNullOrEmpty(donor.Email))
                    {
                        await _emailService.SendEmailAsync(
                            donor.Email,
                            "Urgent: Emergency Blood Donation Request",
                            $"<h2>Emergency Blood Donation Request</h2>" +
                            $"<p>Dear {donor.FirstName},</p>" +
                            $"<p>There is an urgent need for <strong>{emergencyRequest.BloodGroup?.GroupName}</strong> blood at <strong>{emergencyRequest.HospitalName}</strong>.</p>" +
                            $"<p><strong>Patient:</strong> {emergencyRequest.PatientName}<br>" +
                            $"<strong>Urgency:</strong> {emergencyRequest.UrgencyLevel}<br>" +
                            $"<strong>Blood Component:</strong> {emergencyRequest.ComponentType?.Name}<br>" +
                            $"<strong>Location:</strong> {emergencyRequest.Address}</p>" +
                            $"<p>If you are available to donate, please contact <strong>{emergencyRequest.ContactInfo}</strong> or respond through the Blood Donation Support System app.</p>" +
                            $"<p>Thank you for your help in this critical situation.</p>" +
                            $"<p>Regards,<br>Blood Donation Support System</p>"
                        );
                    }
                }
                
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                // Log the error but don't fail the request
                // In a real application, this would be logged
            }
        }
        
        private async Task NotifyAdminsAboutNewEmergencyRequest(EmergencyRequest emergencyRequest)
        {
            try
            {
                // Get admin users
                var adminRole = await _unitOfWork.Roles.GetByNameAsync("Admin");
                if (adminRole == null) return;
                
                var adminUsers = await _unitOfWork.Users.GetUsersByRoleIdAsync(adminRole.Id);
                
                // Create notifications for admin users
                foreach (var admin in adminUsers)
                {
                    var notification = new Notification
                    {
                        UserId = admin.Id,
                        Title = "New Public Emergency Request",
                        Message = $"A new emergency request has been submitted by the public and requires approval. " +
                                  $"Patient: {emergencyRequest.PatientName}, Blood Group: {emergencyRequest.BloodGroup?.GroupName}",
                        Type = "EmergencyRequestApproval",
                        ReferenceId = emergencyRequest.Id.ToString(),
                        IsRead = false,
                        CreatedTime = DateTimeOffset.UtcNow
                    };
                    
                    await _unitOfWork.Notifications.AddAsync(notification);
                    
                    // Send email if user has email
                    if (!string.IsNullOrEmpty(admin.Email))
                    {
                        await _emailService.SendEmailAsync(
                            admin.Email,
                            "New Public Emergency Blood Request Requires Approval",
                            $"<h2>New Public Emergency Request</h2>" +
                            $"<p>A new emergency blood request has been submitted by the public and requires your approval:</p>" +
                            $"<p><strong>Patient:</strong> {emergencyRequest.PatientName}<br>" +
                            $"<strong>Blood Group:</strong> {emergencyRequest.BloodGroup?.GroupName}<br>" +
                            $"<strong>Component:</strong> {emergencyRequest.ComponentType?.Name}<br>" +
                            $"<strong>Hospital:</strong> {emergencyRequest.HospitalName}<br>" +
                            $"<strong>Contact:</strong> {emergencyRequest.ContactInfo}</p>" +
                            $"<p>Please review and approve this request as soon as possible.</p>" +
                            $"<p>Regards,<br>Blood Donation Support System</p>"
                        );
                    }
                }
                
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                // Log the error but don't fail the request
                // In a real application, this would be logged
            }
        }
    }
}