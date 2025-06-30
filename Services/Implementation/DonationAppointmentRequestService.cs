using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.Extensions.Logging;
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
    public class DonationAppointmentRequestService : IDonationAppointmentRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DonationAppointmentRequestService> _logger;
        private readonly INotificationService _notificationService;

        // Default capacity per time slot per location
        private const int DefaultTimeSlotCapacity = 10;

        public DonationAppointmentRequestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DonationAppointmentRequestService> logger,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<DonationAppointmentRequestDto>> UpdateAppointmentStatusAsync(Guid requestId, UpdateAppointmentStatusDto updateDto)
        {
            try
            {
                var request = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                if (request == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.NotFound,
                        "Appointment request not found");
                }

                // Update status
                var statusUpdated = await _unitOfWork.DonationAppointmentRequests
                    .UpdateStatusAsync(requestId, updateDto.Status, updateDto.UpdatedByUserId);

                // Set timing fields based on status
                if (updateDto.Status == "CheckedIn")
                {
                    request.CheckInTime = DateTimeOffset.UtcNow;
                }
                else if (updateDto.Status == "Completed")
                {
                    request.CompletedTime = DateTimeOffset.UtcNow;
                }
                else if (updateDto.Status == "Cancelled")
                {
                    request.CancelledTime = DateTimeOffset.UtcNow;
                }

                // Update note if applicable
                if (!string.IsNullOrWhiteSpace(updateDto.Note))
                {
                    request.Notes = updateDto.Note;
                    _unitOfWork.DonationAppointmentRequests.Update(request);
                }

                await _unitOfWork.CompleteAsync();

                // Handle capacity based on status
                if (updateDto.Status is "Approved" or "Accepted")
                {
                    // Decrease capacity
                    await UpdateLocationCapacityAsync(
                        request.LocationId,
                        request.PreferredTimeSlot,
                        request.PreferredDate,
                        -1
                    );
                }
                else if (updateDto.Status is "Rejected" or "Denied")
                {
                    // Increase capacity back
                    await UpdateLocationCapacityAsync(
                        request.LocationId,
                        request.PreferredTimeSlot,
                        request.PreferredDate,
                        1
                    );
                }

                var updatedRequest = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                var requestDto = _mapper.Map<DonationAppointmentRequestDto>(updatedRequest);

                return new ApiResponse<DonationAppointmentRequestDto>(
                    requestDto,
                    "Appointment status updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating appointment status: {RequestId}", requestId);
                return new ApiResponse<DonationAppointmentRequestDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while updating appointment status");
            }
        }

        public async Task<PagedApiResponse<DonationAppointmentRequestDto>> GetPagedAppointmentRequestsAsync(AppointmentRequestParameters parameters)
        {
            try
            {
                var (requests, totalCount) = await _unitOfWork.DonationAppointmentRequests.GetPagedAppointmentRequestsAsync(parameters);
                var requestDtos = _mapper.Map<IEnumerable<DonationAppointmentRequestDto>>(requests);

                return new PagedApiResponse<DonationAppointmentRequestDto>(
                    requestDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize)
                {
                    Message = $"Retrieved {requestDtos.Count()} appointment requests out of {totalCount} total"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged appointment requests");
                return new PagedApiResponse<DonationAppointmentRequestDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Error occurred while retrieving appointment requests"
                };
            }
        }

        private async Task UpdateLocationCapacityAsync(Guid locationId, string timeSlot, DateTimeOffset date, int delta)
        {
            // Lấy capacity theo location, timeSlot, ngày
            var capacities = await _unitOfWork.LocationCapacities
                .FindAsync(lc =>
                    lc.LocationId == locationId
                    && lc.TimeSlot == timeSlot
                    && lc.IsActive
                    && (lc.DayOfWeek == null || lc.DayOfWeek == date.DayOfWeek)
                    && (lc.EffectiveDate == null || lc.EffectiveDate <= date)
                    && (lc.ExpiryDate == null || lc.ExpiryDate >= date)
                );

            var capacity = capacities.FirstOrDefault();
            if (capacity != null)
            {
                capacity.TotalCapacity += delta;
                if (capacity.TotalCapacity < 0) capacity.TotalCapacity = 0;
                _unitOfWork.LocationCapacities.Update(capacity);
                await _unitOfWork.CompleteAsync();
            }
        }

        public async Task<ApiResponse<DonationAppointmentRequestDto>> GetAppointmentRequestByIdAsync(Guid id)
        {
            try
            {
                var request = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(id);
                if (request == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.NotFound,
                        "Appointment request not found");
                }

                var requestDto = _mapper.Map<DonationAppointmentRequestDto>(request);
                return new ApiResponse<DonationAppointmentRequestDto>(requestDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting appointment request with ID: {Id}", id);
                return new ApiResponse<DonationAppointmentRequestDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while retrieving appointment request");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetAppointmentRequestsByDonorIdAsync(Guid donorId)
        {
            try
            {
                // First verify donor exists
                var donor = await _unitOfWork.DonorProfiles.GetByUserIdAsync(donorId);
                if (donor == null)
                {
                    return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(
                        HttpStatusCode.NotFound,
                        "Donor profile not found");
                }

                var requests = await _unitOfWork.DonationAppointmentRequests.GetRequestsByDonorIdAsync(donor.Id);
                var requestDtos = _mapper.Map<IEnumerable<DonationAppointmentRequestDto>>(requests);

                return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(
                    requestDtos,
                    $"Retrieved {requestDtos.Count()} appointment requests for donor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting appointment requests for donor ID: {DonorId}", donorId);
                return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while retrieving donor appointment requests");
            }
        }

        public async Task<ApiResponse<DonationAppointmentRequestDto>> CreateDonorAppointmentRequestAsync(CreateDonorAppointmentRequestDto requestDto, Guid donorUserId)
        {
            try
            {
                // Get donor profile
                var donor = await _unitOfWork.DonorProfiles.GetByUserIdAsync(donorUserId);
                if (donor == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.NotFound,
                        "Donor profile not found");
                }

                // Validate donor eligibility
                if (donor.NextAvailableDonationDate.HasValue && donor.NextAvailableDonationDate > DateTimeOffset.UtcNow)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.BadRequest,
                        $"Donor is not eligible to donate until {donor.NextAvailableDonationDate:yyyy-MM-dd}");
                }

                // Validate location exists
                var location = await _unitOfWork.Locations.GetByIdAsync(requestDto.LocationId);
                if (location == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.BadRequest,
                        "Invalid location specified");
                }

                // Validate future date
                if (requestDto.PreferredDate <= DateTimeOffset.UtcNow)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.BadRequest,
                        "Preferred date must be in the future");
                }

                // Check for existing pending requests
                var existingRequests = await _unitOfWork.DonationAppointmentRequests.GetRequestsByDonorIdAsync(donor.Id);
                var pendingRequests = existingRequests.Where(r => r.Status == "Pending" || r.Status == "Approved").ToList();
                
                if (pendingRequests.Any())
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.Conflict,
                        "You already have pending appointment requests. Please wait for them to be processed or cancel them first.");
                }

                // Create appointment request
                var appointmentRequest = _mapper.Map<DonationAppointmentRequest>(requestDto);
                appointmentRequest.DonorId = donor.Id;
                appointmentRequest.InitiatedByUserId = donorUserId;
                appointmentRequest.CreatedTime = DateTimeOffset.UtcNow;

                await _unitOfWork.DonationAppointmentRequests.AddAsync(appointmentRequest);
                await _unitOfWork.CompleteAsync();

                // Send notification to staff
                await SendStaffNotificationAsync(appointmentRequest.Id, "New donation appointment request received");

                var createdRequestDto = _mapper.Map<DonationAppointmentRequestDto>(appointmentRequest);
                return new ApiResponse<DonationAppointmentRequestDto>(
                    createdRequestDto,
                    "Appointment request created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating donor appointment request");
                return new ApiResponse<DonationAppointmentRequestDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while creating appointment request");
            }
        }

        public async Task<ApiResponse<DonationAppointmentRequestDto>> CreateStaffAppointmentRequestAsync(CreateStaffAppointmentRequestDto requestDto, Guid staffUserId)
        {
            try
            {
                // Validate donor exists
                var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(requestDto.DonorId);
                if (donor == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.NotFound,
                        "Donor not found");
                }

                // Validate location exists
                var location = await _unitOfWork.Locations.GetByIdAsync(requestDto.LocationId);
                if (location == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.BadRequest,
                        "Invalid location specified");
                }

                // Create staff-initiated appointment request
                var appointmentRequest = _mapper.Map<DonationAppointmentRequest>(requestDto);
                appointmentRequest.InitiatedByUserId = staffUserId;
                appointmentRequest.CreatedTime = DateTimeOffset.UtcNow;

                await _unitOfWork.DonationAppointmentRequests.AddAsync(appointmentRequest);
                await _unitOfWork.CompleteAsync();

                // Send notification to donor
                await SendDonorNotificationAsync(appointmentRequest.Id, "You have been assigned a donation appointment");

                var createdRequestDto = _mapper.Map<DonationAppointmentRequestDto>(appointmentRequest);
                return new ApiResponse<DonationAppointmentRequestDto>(
                    createdRequestDto,
                    "Staff appointment assignment created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating staff appointment request");
                return new ApiResponse<DonationAppointmentRequestDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while creating staff appointment assignment");
            }
        }

        public async Task<ApiResponse<DonationAppointmentRequestDto>> ApproveAppointmentRequestAsync(Guid requestId, StaffAppointmentResponseDto responseDto, Guid staffUserId)
        {
            try
            {
                var request = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                if (request == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.NotFound,
                        "Appointment request not found");
                }

                if (request.Status != "Pending")
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.BadRequest,
                        "Request is not in pending status");
                }

                // Update request with approval details
                await _unitOfWork.DonationAppointmentRequests.UpdateStaffResponseAsync(
                    requestId,
                    responseDto.ConfirmedDate ?? request.PreferredDate,
                    responseDto.ConfirmedTimeSlot ?? request.PreferredTimeSlot,
                    responseDto.ConfirmedLocationId ?? request.LocationId,
                    responseDto.Notes);

                await _unitOfWork.DonationAppointmentRequests.UpdateStatusAsync(requestId, "Approved", staffUserId);
                
                await _unitOfWork.CompleteAsync();
                await UpdateLocationCapacityAsync(
                    request.LocationId,
                    request.PreferredTimeSlot,
                    request.PreferredDate,
                    -1 // giảm capacity
                );
                // Send notification to donor
                await SendDonorNotificationAsync(requestId, "Your donation appointment request has been approved");

                var updatedRequest = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                var requestDto = _mapper.Map<DonationAppointmentRequestDto>(updatedRequest);

                return new ApiResponse<DonationAppointmentRequestDto>(
                    requestDto,
                    "Appointment request approved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while approving appointment request: {RequestId}", requestId);
                return new ApiResponse<DonationAppointmentRequestDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while approving appointment request");
            }
        }

        public async Task<ApiResponse<DonationAppointmentRequestDto>> AcceptStaffAssignmentAsync(Guid requestId, DonorAppointmentResponseDto responseDto, Guid donorUserId)
        {
            try
            {
                var request = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                if (request == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.NotFound,
                        "Appointment request not found");
                }

                // Verify donor ownership
                var donor = await _unitOfWork.DonorProfiles.GetByUserIdAsync(donorUserId);
                if (donor == null || request.DonorId != donor.Id)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.Forbidden,
                        "You can only respond to your own appointment assignments");
                }

                if (request.RequestType != "StaffInitiated" || request.Status != "Pending")
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.BadRequest,
                        "This request cannot be responded to");
                }

                // Update donor response
                await _unitOfWork.DonationAppointmentRequests.UpdateDonorResponseAsync(
                    requestId, responseDto.Accepted, responseDto.Notes);
                await _unitOfWork.CompleteAsync();

                // Send notification to staff
                var message = responseDto.Accepted 
                    ? "Donor has accepted the appointment assignment" 
                    : "Donor has rejected the appointment assignment";
                if (responseDto.Accepted)
                {
                    await UpdateLocationCapacityAsync(
                        request.LocationId,
                        request.PreferredTimeSlot,
                        request.PreferredDate,
                        -1
                    );
                }
                await SendStaffNotificationAsync(requestId, message);

                var updatedRequest = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                var requestDto = _mapper.Map<DonationAppointmentRequestDto>(updatedRequest);

                return new ApiResponse<DonationAppointmentRequestDto>(
                    requestDto,
                    "Response recorded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing donor response: {RequestId}", requestId);
                return new ApiResponse<DonationAppointmentRequestDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while processing response");
            }
        }

        public async Task<ApiResponse<IEnumerable<AvailableTimeSlotsDto>>> GetAvailableTimeSlotsAsync(Guid locationId, DateTimeOffset startDate, int days = 7)
        {
            try
            {
                var location = await _unitOfWork.Locations.GetByIdAsync(locationId);
                if (location == null)
                {
                    return new ApiResponse<IEnumerable<AvailableTimeSlotsDto>>(
                        HttpStatusCode.NotFound,
                        "Location not found");
                }

                var availableSlots = new List<AvailableTimeSlotsDto>();

                for (int i = 0; i < days; i++)
                {
                    var date = startDate.AddDays(i);
                    var dayOfWeek = date.DayOfWeek;
                    
                    // Get location capacities for this date and day of week
                    var locationCapacities = await _unitOfWork.LocationCapacities
                        .FindAsync(lc => lc.LocationId == locationId && 
                                        lc.IsActive && 
                                        (lc.DayOfWeek == null || lc.DayOfWeek == dayOfWeek) &&
                                        (lc.EffectiveDate == null || lc.EffectiveDate <= date) &&
                                        (lc.ExpiryDate == null || lc.ExpiryDate >= date));

                    // Get existing bookings for this date
                    var timeSlotCounts = await _unitOfWork.DonationAppointmentRequests.GetTimeSlotCapacityAsync(locationId, date);

                    var daySlots = new AvailableTimeSlotsDto
                    {
                        LocationId = locationId,
                        LocationName = location.Name,
                        Date = date,
                        AvailableSlots = new List<TimeSlotDto>()
                    };

                    // Create time slots based on capacities or use defaults
                    var timeSlots = new[] { "Morning", "Afternoon", "Evening" };
                    
                    foreach (var timeSlot in timeSlots)
                    {
                        // Find specific capacity for this time slot
                        var capacity = locationCapacities.FirstOrDefault(lc => lc.TimeSlot == timeSlot);
                        var totalCapacity = capacity?.TotalCapacity ?? DefaultTimeSlotCapacity;
                        var bookedCount = timeSlotCounts.GetValueOrDefault(timeSlot, 0);

                        daySlots.AvailableSlots.Add(new TimeSlotDto
                        {
                            TimeSlot = timeSlot,
                            AvailableCapacity = Math.Max(0, totalCapacity - bookedCount),
                            TotalCapacity = totalCapacity,
                            IsAvailable = bookedCount < totalCapacity
                        });
                    }

                    availableSlots.Add(daySlots);
                }

                return new ApiResponse<IEnumerable<AvailableTimeSlotsDto>>(
                    availableSlots,
                    $"Retrieved available time slots for {days} days");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting available time slots for location: {LocationId}", locationId);
                return new ApiResponse<IEnumerable<AvailableTimeSlotsDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while retrieving available time slots");
            }
        }

        // Additional required methods with basic implementations
        public async Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetPendingStaffReviewsAsync()
        {
            try
            {
                var requests = await _unitOfWork.DonationAppointmentRequests.GetPendingStaffReviewsAsync();
                var requestDtos = _mapper.Map<IEnumerable<DonationAppointmentRequestDto>>(requests);
                return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(requestDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending staff reviews");
                return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(
                    HttpStatusCode.InternalServerError, "Error retrieving pending reviews");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetPendingDonorResponsesAsync()
        {
            try
            {
                var requests = await _unitOfWork.DonationAppointmentRequests.GetPendingDonorResponsesAsync();
                var requestDtos = _mapper.Map<IEnumerable<DonationAppointmentRequestDto>>(requests);
                return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(requestDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending donor responses");
                return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(
                    HttpStatusCode.InternalServerError, "Error retrieving pending donor responses");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetUrgentRequestsAsync()
        {
            try
            {
                var requests = await _unitOfWork.DonationAppointmentRequests.GetUrgentRequestsAsync();
                var requestDtos = _mapper.Map<IEnumerable<DonationAppointmentRequestDto>>(requests);
                return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(requestDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting urgent requests");
                return new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(
                    HttpStatusCode.InternalServerError, "Error retrieving urgent requests");
            }
        }

        // Placeholder implementations for other required methods
        public Task<ApiResponse<DonationAppointmentRequestDto>> UpdateDonorAppointmentRequestAsync(Guid requestId, UpdateAppointmentRequestDto updateDto, Guid donorUserId) => throw new NotImplementedException();
        public Task<ApiResponse<DonationAppointmentRequestDto>> UpdateStaffAppointmentRequestAsync(Guid requestId, UpdateAppointmentRequestDto updateDto, Guid staffUserId) => throw new NotImplementedException();
        public async Task<ApiResponse<DonationAppointmentRequestDto>> RejectAppointmentRequestAsync(Guid requestId, StaffAppointmentResponseDto responseDto, Guid staffUserId)
        {
            try
            {
                var request = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                if (request == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.NotFound,
                        "Appointment request not found");
                }

                if (request.Status != "Pending")
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.BadRequest,
                        "Request is not in pending status");
                }

                // Update status to Rejected
                await _unitOfWork.DonationAppointmentRequests.UpdateStatusAsync(requestId, "Rejected", staffUserId);
                await _unitOfWork.CompleteAsync();

                // Tăng lại capacity
                await UpdateLocationCapacityAsync(
                    request.LocationId,
                    request.PreferredTimeSlot,
                    request.PreferredDate,
                    1
                );

                // Send notification to donor
                await SendDonorNotificationAsync(requestId, "Your donation appointment request has been rejected");

                var updatedRequest = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                var requestDto = _mapper.Map<DonationAppointmentRequestDto>(updatedRequest);

                return new ApiResponse<DonationAppointmentRequestDto>(
                    requestDto,
                    "Appointment request rejected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rejecting appointment request: {RequestId}", requestId);
                return new ApiResponse<DonationAppointmentRequestDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while rejecting appointment request");
            }
        }

        public async Task<ApiResponse<DonationAppointmentRequestDto>> RejectStaffAssignmentAsync(Guid requestId, DonorAppointmentResponseDto responseDto, Guid donorUserId)
        {
            try
            {
                var request = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                if (request == null)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.NotFound,
                        "Appointment request not found");
                }

                // Verify donor ownership
                var donor = await _unitOfWork.DonorProfiles.GetByUserIdAsync(donorUserId);
                if (donor == null || request.DonorId != donor.Id)
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.Forbidden,
                        "You can only respond to your own appointment assignments");
                }

                if (request.RequestType != "StaffInitiated" || request.Status != "Pending")
                {
                    return new ApiResponse<DonationAppointmentRequestDto>(
                        HttpStatusCode.BadRequest,
                        "This request cannot be responded to");
                }

                // Update donor response to rejected
                await _unitOfWork.DonationAppointmentRequests.UpdateDonorResponseAsync(
                    requestId, false, responseDto.Notes);
                await _unitOfWork.DonationAppointmentRequests.UpdateStatusAsync(requestId, "Rejected", donorUserId);
                await _unitOfWork.CompleteAsync();

                // Tăng lại capacity
                await UpdateLocationCapacityAsync(
                    request.LocationId,
                    request.PreferredTimeSlot,
                    request.PreferredDate,
                    1
                );

                // Send notification to staff
                await SendStaffNotificationAsync(requestId, "Donor has rejected the appointment assignment");

                var updatedRequest = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                var requestDto = _mapper.Map<DonationAppointmentRequestDto>(updatedRequest);

                return new ApiResponse<DonationAppointmentRequestDto>(
                    requestDto,
                    "Staff assignment rejected successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while rejecting staff assignment: {RequestId}", requestId);
                return new ApiResponse<DonationAppointmentRequestDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while rejecting staff assignment");
            }
        }

        public async Task<ApiResponse> CancelDonorAppointmentRequestAsync(Guid requestId, Guid donorUserId)
        {
            try
            {
                var request = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                if (request == null)
                {
                    return new ApiResponse(
                        HttpStatusCode.NotFound,
                        "Appointment request not found");
                }

                // Verify donor ownership
                var donor = await _unitOfWork.DonorProfiles.GetByUserIdAsync(donorUserId);
                if (donor == null || request.DonorId != donor.Id)
                {
                    return new ApiResponse(
                        HttpStatusCode.Forbidden,
                        "You can only cancel your own appointment requests");
                }

                if (request.Status != "Pending" && request.Status != "Approved")
                {
                    return new ApiResponse(
                        HttpStatusCode.BadRequest,
                        "Only pending or approved requests can be cancelled");
                }

                // Update status to Cancelled
                await _unitOfWork.DonationAppointmentRequests.UpdateStatusAsync(requestId, "Cancelled", donorUserId);
                await _unitOfWork.CompleteAsync();

                // Tăng lại capacity
                await UpdateLocationCapacityAsync(
                    request.LocationId,
                    request.PreferredTimeSlot,
                    request.PreferredDate,
                    1
                );

                // Send notification to staff
                await SendStaffNotificationAsync(requestId, "Donor has cancelled the appointment request");

                return new ApiResponse("Appointment request cancelled successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cancelling appointment request: {RequestId}", requestId);
                return new ApiResponse(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while cancelling appointment request");
            }
        }
        public Task<ApiResponse<DonationAppointmentRequestDto>> ModifyAppointmentRequestAsync(Guid requestId, StaffAppointmentResponseDto responseDto, Guid staffUserId) => throw new NotImplementedException();
        public Task<ApiResponse<DonationAppointmentRequestDto>> ConvertToWorkflowAsync(Guid requestId, Guid staffUserId) => throw new NotImplementedException();
        public Task<ApiResponse> LinkToWorkflowAsync(Guid requestId, Guid workflowId) => throw new NotImplementedException();
        public Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetAppointmentsByLocationAndDateAsync(Guid locationId, DateTimeOffset date) => throw new NotImplementedException();
        public Task<ApiResponse<int>> MarkExpiredRequestsAsync() => throw new NotImplementedException();
        public Task<ApiResponse<IEnumerable<DonationAppointmentRequestDto>>> GetRequestsExpiringInHoursAsync(int hours) => throw new NotImplementedException();
        public Task<ApiResponse> SendAppointmentReminderAsync(Guid requestId) => throw new NotImplementedException();
        public Task<ApiResponse> SendExpiryWarningAsync(Guid requestId) => throw new NotImplementedException();

        #region Helper Methods

        private async Task SendDonorNotificationAsync(Guid requestId, string message)
        {
            try
            {
                var request = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(requestId);
                if (request?.Donor?.User != null)
                {
                    var notificationDto = new CreateNotificationDto
                    {
                        UserId = request.Donor.User.Id,
                        Title = "Donation Appointment Update",
                        Message = message,
                        Type = "AppointmentUpdate"
                    };

                    await _notificationService.CreateNotificationAsync(notificationDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending donor notification for request: {RequestId}", requestId);
            }
        }

        private async Task SendStaffNotificationAsync(Guid requestId, string message)
        {
            try
            {
                // Send to all staff members (Admin and Staff roles)
                // This would typically query staff users and send notifications
                _logger.LogInformation("Staff notification sent for request {RequestId}: {Message}", requestId, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending staff notification for request: {RequestId}", requestId);
            }
        }

        #endregion
    }
}