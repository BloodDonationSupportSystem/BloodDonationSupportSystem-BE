using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Repositories.Interface;
using Services.Interface;
using Shared.Constants;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class DonationEventService : IDonationEventService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<DonationEventService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IBloodRequestService _bloodRequestService;
        private readonly IBloodInventoryService _bloodInventoryService;
        private readonly IRealTimeNotificationService _realTimeNotificationService;

        public DonationEventService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<DonationEventService> logger,
            INotificationService notificationService,
            IBloodRequestService bloodRequestService,
            IBloodInventoryService bloodInventoryService,
            IRealTimeNotificationService realTimeNotificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
            _bloodRequestService = bloodRequestService;
            _bloodInventoryService = bloodInventoryService;
            _realTimeNotificationService = realTimeNotificationService;
        }

        public async Task<PagedApiResponse<DonationEventDto>> GetPagedDonationEventsAsync(DonationEventParameters parameters)
        {
            try
            {
                var (donationEvents, totalCount) = await _unitOfWork.DonationEvents.GetPagedDonationEventsAsync(parameters);
                var donationEventDtos = _mapper.Map<IEnumerable<DonationEventDto>>(donationEvents);
                
                return new PagedApiResponse<DonationEventDto>(
                    donationEventDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting paged donation events");
                var errorResponse = new PagedApiResponse<DonationEventDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "Error occurred while getting donation events"
                };
                return errorResponse;
            }
        }

        public async Task<ApiResponse<DonationEventDto>> GetDonationEventByIdAsync(Guid id)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(id);
                if (donationEvent == null || donationEvent.DeletedTime != null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Donation event not found");
                }

                var donationEventDto = _mapper.Map<DonationEventDto>(donationEvent);
                return new ApiResponse<DonationEventDto>(donationEventDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting donation event with ID: {Id}", id);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting donation event");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> GetDonationEventByAppointmentIdAsync(Guid appointmentId)
        {
            try
            {
                // First, check if the appointment exists
                var appointment = await _unitOfWork.DonationAppointmentRequests.GetByIdAsync(appointmentId);
                if (appointment == null || appointment.DeletedTime != null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Appointment not found");
                }
                
                // Find the donation event linked to this appointment
                // In our system, when an appointment is checked in, a donation event is created with:
                // - RequestType = "Appointment"
                // - RequestId = appointment.Id
                var donationEvents = await _unitOfWork.DonationEvents.FindAsync(d => 
                    d.RequestId == appointmentId && 
                    d.RequestType == "Appointment" && 
                    d.DeletedTime == null);
                
                var donationEvent = donationEvents.FirstOrDefault();
                
                if (donationEvent == null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "No donation event found for this appointment. The appointment may not have been checked in yet.");
                }

                // Get the full details of the donation event
                var donationEventWithDetails = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                var donationEventDto = _mapper.Map<DonationEventDto>(donationEventWithDetails);
                
                return new ApiResponse<DonationEventDto>(
                    donationEventDto,
                    "Donation event retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting donation event for appointment ID: {AppointmentId}", appointmentId);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting donation event for appointment");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationEventDto>>> GetDonationEventsByRequestAsync(Guid requestId, string requestType)
        {
            try
            {
                var donationEvents = await _unitOfWork.DonationEvents.FindAsync(d => 
                    d.RequestId == requestId && d.RequestType == requestType && d.DeletedTime == null);
                var donationEventDtos = _mapper.Map<IEnumerable<DonationEventDto>>(donationEvents);
                
                return new ApiResponse<IEnumerable<DonationEventDto>>(
                    donationEventDtos,
                    $"Found {donationEventDtos.Count()} donation events for the {requestType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting donation events for request. RequestId: {RequestId}, RequestType: {RequestType}",
                    requestId, requestType);
                return new ApiResponse<IEnumerable<DonationEventDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting donation events for request");
            }
        }

        public async Task<ApiResponse<IEnumerable<DonationEventDto>>> GetDonationEventsByDonorAsync(Guid donorId)
        {
            try
            {
                var donationEvents = await _unitOfWork.DonationEvents.FindAsync(d => 
                    d.DonorId == donorId && d.DeletedTime == null);
                var donationEventDtos = _mapper.Map<IEnumerable<DonationEventDto>>(donationEvents);
                
                return new ApiResponse<IEnumerable<DonationEventDto>>(
                    donationEventDtos,
                    $"Found {donationEventDtos.Count()} donation events for the donor");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting donation events for donor. DonorId: {DonorId}", donorId);
                return new ApiResponse<IEnumerable<DonationEventDto>>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while getting donation events for donor");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> CreateDonationEventAsync(CreateDonationEventDto donationEventDto)
        {
            try
            {
                // Chỉ sử dụng BloodRequest cho cả yêu cầu thường và khẩn cấp
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(donationEventDto.RequestId);
                if (bloodRequest == null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Blood request not found");
                }
                
                var bloodGroupId = bloodRequest.BloodGroupId;
                var componentTypeId = bloodRequest.ComponentTypeId;
                var requiredQuantity = bloodRequest.QuantityUnits;
                
                // Create a new donation event - không còn kiểm tra inventory ở đây
                var donationEvent = new DonationEvent
                {
                    Id = Guid.NewGuid(),
                    RequestId = donationEventDto.RequestId,
                    RequestType = "BloodRequest", // Luôn là BloodRequest
                    BloodGroupId = bloodGroupId,
                    ComponentTypeId = componentTypeId,
                    LocationId = donationEventDto.LocationId,
                    Status = "Created",
                    Notes = donationEventDto.Notes,
                    CreatedTime = DateTimeOffset.UtcNow,
                    IsActive = true
                };
                
                // Không còn logic CheckInventoryFirst - việc fulfill từ inventory được xử lý ở BloodRequestService
                
                // Save the donation event
                await _unitOfWork.DonationEvents.AddAsync(donationEvent);
                await _unitOfWork.CompleteAsync();
                
                // Send real-time notifications
                await SendDonationEventNotificationAsync(donationEvent, "New donation event created and awaiting donor assignment");
                
                // Send realtime notification to staff for emergency requests
                if (bloodRequest.IsEmergency)
                {
                    await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                        bloodRequest.Id, 
                        "Processing", 
                        "Emergency request is being processed - donation event created");
                    await _realTimeNotificationService.UpdateEmergencyDashboard();
                }
                
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                var result = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                var donationEventResultDto = _mapper.Map<DonationEventDto>(result);
                
                return new ApiResponse<DonationEventDto>(
                    donationEventResultDto,
                    "Donation event created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating donation event for request ID: {RequestId}", 
                    donationEventDto.RequestId);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while creating donation event");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> CreateWalkInDonationEventAsync(CreateWalkInDonationEventDto walkInDto)
        {
            try
            {
                // 1. Kiểm tra người hiến máu đã có trong hệ thống chưa (chỉ dựa trên số điện thoại)
                var existingUser = await _unitOfWork.Users.FindAsync(u => 
                    u.PhoneNumber == walkInDto.DonorInfo.PhoneNumber);
                var user = existingUser.FirstOrDefault();
                
                DonorProfile donorProfile;
                bool isNewUser = false;

                if (user != null)
                {
                    // Người dùng đã tồn tại - cập nhật thông tin donor profile
                    donorProfile = await _unitOfWork.DonorProfiles.GetByUserIdAsync(user.Id);
                    
                    if (donorProfile == null)
                    {
                        // User tồn tại nhưng chưa có donor profile
                        donorProfile = new DonorProfile
                        {
                            Id = Guid.NewGuid(),
                            UserId = user.Id,
                            BloodGroupId = walkInDto.DonorInfo.BloodGroupId,
                            DateOfBirth = walkInDto.DonorInfo.DateOfBirth,
                            Address = walkInDto.DonorInfo.Address ?? string.Empty,
                            LastDonationDate = walkInDto.DonorInfo.LastDonationDate,
                            CreatedTime = DateTimeOffset.UtcNow
                        };
                        
                        await _unitOfWork.DonorProfiles.AddAsync(donorProfile);
                    }
                    else
                    {
                        // Cập nhật thông tin donor profile nếu cần
                        if (donorProfile.BloodGroupId != walkInDto.DonorInfo.BloodGroupId)
                        {
                            donorProfile.BloodGroupId = walkInDto.DonorInfo.BloodGroupId;
                        }
                        
                        if (walkInDto.DonorInfo.LastDonationDate.HasValue && 
                            (!donorProfile.LastDonationDate.HasValue || 
                             donorProfile.LastDonationDate.Value < walkInDto.DonorInfo.LastDonationDate.Value))
                        {
                            donorProfile.LastDonationDate = walkInDto.DonorInfo.LastDonationDate;
                        }
                        
                        if (!string.IsNullOrEmpty(walkInDto.DonorInfo.Address))
                        {
                            donorProfile.Address = walkInDto.DonorInfo.Address;
                        }
                        
                        _unitOfWork.DonorProfiles.Update(donorProfile);
                    }
                    
                    // Cập nhật thông tin cơ bản của user nếu cần
                    bool userUpdated = false;
                    if (!string.IsNullOrEmpty(walkInDto.DonorInfo.FirstName) && user.FirstName != walkInDto.DonorInfo.FirstName)
                    {
                        user.FirstName = walkInDto.DonorInfo.FirstName;
                        userUpdated = true;
                    }
                    if (!string.IsNullOrEmpty(walkInDto.DonorInfo.LastName) && user.LastName != walkInDto.DonorInfo.LastName)
                    {
                        user.LastName = walkInDto.DonorInfo.LastName;
                        userUpdated = true;
                    }
                    if (!string.IsNullOrEmpty(walkInDto.DonorInfo.Email) && user.Email != walkInDto.DonorInfo.Email)
                    {
                        // Kiểm tra email mới có bị trùng không
                        var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == walkInDto.DonorInfo.Email && u.Id != user.Id);
                        if (!emailExists.Any())
                        {
                            user.Email = walkInDto.DonorInfo.Email;
                            userUpdated = true;
                        }
                    }
                    
                    if (userUpdated)
                    {
                        _unitOfWork.Users.Update(user);
                    }
                }
                else
                {
                    // Tạo user mới cho walk-in donor
                    isNewUser = true;
                    
                    var memberRole = await _unitOfWork.Roles.GetByNameAsync("Member");
                    if (memberRole == null)
                    {
                        return new ApiResponse<DonationEventDto>(
                            HttpStatusCode.BadRequest,
                            "Member role not found in the system");
                    }
                    
                    // Tạo UserName unique cho walk-in user
                    string uniqueUserName = $"walkin_{DateTimeOffset.UtcNow:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..8]}";
                    
                    // Đảm bảo UserName không bị trùng (rất hiếm khi xảy ra nhưng vẫn cần check)
                    var existingUserName = await _unitOfWork.Users.FindAsync(u => u.UserName == uniqueUserName);
                    while (existingUserName.Any())
                    {
                        uniqueUserName = $"walkin_{DateTimeOffset.UtcNow:yyyyMMdd}_{Guid.NewGuid().ToString("N")[..8]}";
                        existingUserName = await _unitOfWork.Users.FindAsync(u => u.UserName == uniqueUserName);
                    }
                    
                    // Kiểm tra email có bị trùng không (nếu có email)
                    if (!string.IsNullOrEmpty(walkInDto.DonorInfo.Email))
                    {
                        var emailExists = await _unitOfWork.Users.FindAsync(u => u.Email == walkInDto.DonorInfo.Email);
                        if (emailExists.Any())
                        {
                            return new ApiResponse<DonationEventDto>(
                                HttpStatusCode.Conflict,
                                $"Email '{walkInDto.DonorInfo.Email}' is already registered. Please use different contact information.");
                        }
                    }
                    
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        UserName = uniqueUserName,
                        FirstName = walkInDto.DonorInfo.FirstName ?? string.Empty,
                        LastName = walkInDto.DonorInfo.LastName ?? string.Empty,
                        PhoneNumber = walkInDto.DonorInfo.PhoneNumber,
                        Email = walkInDto.DonorInfo.Email, // Có thể null
                        Password = "WALK_IN_USER", // Mật khẩu placeholder cho walk-in user
                        RoleId = memberRole.Id,
                        CreatedTime = DateTimeOffset.UtcNow,
                        IsActivated = true,
                        LastLogin = DateTimeOffset.UtcNow
                    };
                    
                    await _unitOfWork.Users.AddAsync(user);
                    
                    donorProfile = new DonorProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        BloodGroupId = walkInDto.DonorInfo.BloodGroupId,
                        DateOfBirth = walkInDto.DonorInfo.DateOfBirth,
                        Address = walkInDto.DonorInfo.Address ?? string.Empty,
                        LastDonationDate = walkInDto.DonorInfo.LastDonationDate,
                        CreatedTime = DateTimeOffset.UtcNow
                    };
                    
                    await _unitOfWork.DonorProfiles.AddAsync(donorProfile);
                }
                
                // 2. Tạo donation event cho hiến máu trực tiếp
                var donationEvent = new DonationEvent
                {
                    Id = Guid.NewGuid(),
                    DonorId = donorProfile.Id,
                    LocationId = walkInDto.LocationId,
                    StaffId = walkInDto.StaffId,
                    BloodGroupId = walkInDto.DonorInfo.BloodGroupId,
                    ComponentTypeId = walkInDto.ComponentTypeId,
                    Status = "WalkIn",
                    RequestType = "DirectDonation",
                    Notes = walkInDto.Notes ?? "Walk-in donor",
                    CreatedTime = DateTimeOffset.UtcNow,
                    IsActive = true
                };
                
                await _unitOfWork.DonationEvents.AddAsync(donationEvent);
                await _unitOfWork.CompleteAsync();
                
                // Gửi thông báo nếu là người dùng cũ và có email
                if (!isNewUser && !string.IsNullOrEmpty(user.Email))
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = user.Id,
                        Title = "Walk-In Donation Started",
                        Type = "WalkInDonation",
                        Message = $"Thank you for visiting our blood donation center today. Your walk-in donation process has been started."
                    });
                }
                
                // Send real-time notification for walk-in donation
                await _realTimeNotificationService.NotifyDonationEventStatusChange(
                    donationEvent.Id, "WalkIn", "New walk-in donor checked in");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                var result = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                var donationEventResultDto = _mapper.Map<DonationEventDto>(result);
                
                return new ApiResponse<DonationEventDto>(
                    donationEventResultDto,
                    "Walk-in donation event created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating walk-in donation event");
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while creating walk-in donation event");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> UpdateDonationEventAsync(Guid id, UpdateDonationEventDto donationEventDto)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdAsync(id);
                if (donationEvent == null || donationEvent.DeletedTime != null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Donation event not found");
                }
                
                string previousStatus = donationEvent.Status;
                
                // Update donation event properties
                if (!string.IsNullOrEmpty(donationEventDto.Status))
                {
                    donationEvent.Status = donationEventDto.Status;
                }
                
                if (donationEventDto.DonorId.HasValue)
                {
                    var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(donationEventDto.DonorId.Value);
                    if (donor == null)
                    {
                        return new ApiResponse<DonationEventDto>(
                            HttpStatusCode.BadRequest,
                            "Specified donor profile does not exist");
                    }
                    
                    donationEvent.DonorId = donationEventDto.DonorId;
                    
                    if (donationEvent.Status == "Created")
                    {
                        donationEvent.Status = "DonorAssigned";
                    }
                }
                
                if (donationEventDto.AppointmentDate.HasValue)
                {
                    donationEvent.AppointmentDate = donationEventDto.AppointmentDate;
                    donationEvent.AppointmentLocation = donationEventDto.AppointmentLocation;
                    
                    if (donationEvent.Status == "DonorAssigned")
                    {
                        donationEvent.Status = "Scheduled";
                    }
                }
                
                if (!string.IsNullOrEmpty(donationEventDto.Notes))
                {
                    donationEvent.Notes = donationEventDto.Notes;
                }
                
                donationEvent.LastUpdatedTime = DateTimeOffset.UtcNow;
                
                _unitOfWork.DonationEvents.Update(donationEvent);
                await _unitOfWork.CompleteAsync();
                
                // Send real-time notifications for status change
                await SendStatusChangeNotificationAsync(donationEvent, previousStatus);
                await _realTimeNotificationService.NotifyDonationEventStatusChange(
                    id, donationEvent.Status, $"Donation event updated: {donationEvent.Status}");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                var updatedDonationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(id);
                var donationEventResultDto = _mapper.Map<DonationEventDto>(updatedDonationEvent);
                
                return new ApiResponse<DonationEventDto>(
                    donationEventResultDto,
                    "Donation event updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating donation event with ID: {Id}", id);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while updating donation event");
            }
        }

        public async Task<ApiResponse> DeleteDonationEventAsync(Guid id)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdAsync(id);
                if (donationEvent == null || donationEvent.DeletedTime != null)
                {
                    return new ApiResponse(
                        HttpStatusCode.NotFound,
                        "Donation event not found");
                }
                
                // Soft delete
                donationEvent.DeletedTime = DateTimeOffset.UtcNow;
                donationEvent.IsActive = false;
                
                _unitOfWork.DonationEvents.Update(donationEvent);
                await _unitOfWork.CompleteAsync();
                
                // Send real-time notification
                await _realTimeNotificationService.NotifyDonationEventStatusChange(
                    id, "Deleted", "Donation event has been deleted");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                return new ApiResponse("Donation event deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting donation event with ID: {Id}", id);
                return new ApiResponse(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while deleting donation event");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> CheckInAppointmentAsync(CheckInAppointmentDto checkInDto)
        {
            try
            {
                // Lấy thông tin lịch hẹn
                var appointment = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(checkInDto.AppointmentId);
                if (appointment == null || appointment.DeletedTime != null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Appointment request not found");
                }
                
                // Kiểm tra trạng thái lịch hẹn
                if (appointment.Status != "Approved" && appointment.Status != "Accepted")
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.BadRequest,
                        "Only confirmed appointments can be checked in");
                }
                
                // Xác định ComponentTypeId hợp lệ
                Guid componentTypeId;
                if (appointment.ComponentTypeId.HasValue)
                {
                    componentTypeId = appointment.ComponentTypeId.Value;
                }
                else
                {
                    // Tìm loại máu "Whole Blood" (Máu toàn phần) là mặc định
                    var wholeBloodComponent = await _unitOfWork.ComponentTypes.FindAsync(ct => ct.Name == "Whole Blood");
                    var defaultComponentType = wholeBloodComponent.FirstOrDefault();
                    
                    if (defaultComponentType == null)
                    {
                        // Nếu không tìm thấy "Whole Blood", lấy bất kỳ loại máu đầu tiên nào
                        var anyComponentType = await _unitOfWork.ComponentTypes.GetAllAsync();
                        defaultComponentType = anyComponentType.FirstOrDefault();
                        
                        if (defaultComponentType == null)
                        {
                            return new ApiResponse<DonationEventDto>(
                                HttpStatusCode.BadRequest,
                                "No component types found in the system. Cannot create donation event.");
                        }
                    }
                    
                    componentTypeId = defaultComponentType.Id;
                    _logger.LogInformation("Using default component type {ComponentTypeName} (ID: {ComponentTypeId}) for donation event", 
                        defaultComponentType.Name, componentTypeId);
                }
                
                // Cập nhật trạng thái lịch hẹn
                appointment.Status = "CheckedIn";
                appointment.CheckInTime = checkInDto.CheckInTime;
                appointment.Notes = (appointment.Notes ?? "") + "\n" + (checkInDto.Notes ?? "");
                
                _unitOfWork.DonationAppointmentRequests.Update(appointment);
                
                // Tạo donation event mới từ thông tin lịch hẹn
                var donationEvent = new DonationEvent
                {
                    Id = Guid.NewGuid(),
                    DonorId = appointment.DonorId,
                    BloodGroupId = appointment.BloodGroupId ?? appointment.Donor.BloodGroupId,
                    ComponentTypeId = componentTypeId,
                    LocationId = appointment.ConfirmedLocationId ?? appointment.LocationId,
                    Status = "CheckedIn",
                    RequestType = "Appointment",
                    RequestId = appointment.Id, // Liên kết với lịch hẹn
                    AppointmentDate = appointment.ConfirmedDate ?? appointment.PreferredDate,
                    AppointmentLocation = appointment.ConfirmedLocation?.Name ?? appointment.Location?.Name ?? string.Empty,
                    AppointmentConfirmed = true,
                    Notes = checkInDto.Notes ?? "Check-in from appointment",
                    CreatedTime = DateTimeOffset.UtcNow,
                    CheckInTime = checkInDto.CheckInTime,
                    IsActive = true
                };
                
                await _unitOfWork.DonationEvents.AddAsync(donationEvent);
                await _unitOfWork.CompleteAsync();
                
                // Update related blood request status
                if (appointment.RelatedBloodRequestId.HasValue)
                {
                    await _bloodRequestService.UpdateBloodRequestStatusWithNotesAsync(
                        appointment.RelatedBloodRequestId.Value, 
                        "DonationInProgress", 
                        $"Donor đã check-in thành công và bắt đầu quá trình hiến máu tại {appointment.ConfirmedLocation?.Name ?? appointment.Location?.Name}");
                }
                
                // Gửi thông báo
                var donor = appointment.Donor;
                if (donor != null && donor.UserId != Guid.Empty)
                {
                    await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = donor.UserId,
                        Title = "Check-in Successful",
                        Type = "AppointmentCheckIn",
                        Message = $"You have successfully checked in for your donation appointment at {appointment.ConfirmedLocation?.Name ?? appointment.Location?.Name}."
                    });
                }
                
                // Send real-time notification for appointment check-in
                await _realTimeNotificationService.NotifyDonationEventStatusChange(
                    donationEvent.Id, "CheckedIn", "Appointment donor checked in successfully");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                var result = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                var donationEventResultDto = _mapper.Map<DonationEventDto>(result);
                
                return new ApiResponse<DonationEventDto>(
                    donationEventResultDto,
                    "Appointment checked in and donation event created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking in appointment: {AppointmentId}", checkInDto.AppointmentId);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while checking in appointment");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> PerformHealthCheckAsync(DonorHealthCheckDto healthCheckDto)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(healthCheckDto.DonationEventId);
                if (donationEvent == null || donationEvent.DeletedTime != null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Donation event not found");
                }
                
                // Kiểm tra trạng thái hiện tại
                if (donationEvent.Status != "CheckedIn" && donationEvent.Status != "WalkIn")
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.BadRequest,
                        "Donation event must be in 'CheckedIn' or 'WalkIn' status to perform health check");
                }
                
                // Cập nhật thông tin sức khỏe
                donationEvent.BloodPressure = healthCheckDto.BloodPressure;
                donationEvent.Temperature = healthCheckDto.Temperature;
                donationEvent.HemoglobinLevel = healthCheckDto.HemoglobinLevel;
                donationEvent.Weight = healthCheckDto.Weight;
                donationEvent.Height = healthCheckDto.Height;
                donationEvent.MedicalNotes = healthCheckDto.MedicalNotes;
                donationEvent.LastUpdatedTime = DateTimeOffset.UtcNow;
                
                // Xử lý trường hợp không đủ điều kiện sức khỏe
                if (!healthCheckDto.IsEligible)
                {
                    donationEvent.Status = "HealthCheckFailed";
                    donationEvent.StatusDescription = "Donor did not meet health requirements";
                    donationEvent.RejectionReason = healthCheckDto.RejectionReason;
                    
                    _unitOfWork.DonationEvents.Update(donationEvent);
                    await _unitOfWork.CompleteAsync();
                    
                    // Cập nhật lịch hẹn nếu có
                    if (donationEvent.RequestType == "Appointment" && donationEvent.RequestId.HasValue)
                    {
                        var appointment = await _unitOfWork.DonationAppointmentRequests.GetByIdAsync(donationEvent.RequestId.Value);
                        if (appointment != null)
                        {
                            appointment.Status = "Rejected";
                            appointment.RejectionReason = healthCheckDto.RejectionReason;
                            _unitOfWork.DonationAppointmentRequests.Update(appointment);
                            
                            // Update related blood request status when health check fails
                            if (appointment.RelatedBloodRequestId.HasValue)
                            {
                                await _bloodRequestService.UpdateBloodRequestStatusWithNotesAsync(
                                    appointment.RelatedBloodRequestId.Value, 
                                    "Processing", 
                                    $"Health check thất bại. Lý do: {healthCheckDto.RejectionReason}. Cần tìm donor khác.");
                            }
                            
                            await _unitOfWork.CompleteAsync();
                        }
                    }
                    
                    // Gửi thông báo
                    if (donationEvent.DonorId.HasValue)
                    {
                        var donor = donationEvent.DonorProfile;
                        if (donor != null && donor.UserId != Guid.Empty)
                        {
                            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                            {
                                UserId = donor.UserId,
                                Title = "Health Check Result",
                                Type = "HealthCheckFailed",
                                Message = $"Unfortunately, you are not eligible to donate blood at this time due to: {healthCheckDto.RejectionReason}"
                            });
                        }
                    }
                    
                    // Send real-time notification for failed health check
                    await _realTimeNotificationService.NotifyDonationEventStatusChange(
                        donationEvent.Id, "HealthCheckFailed", $"Health check failed: {healthCheckDto.RejectionReason}");
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                    
                    var updatedDonationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                    var donationEventResultDto = _mapper.Map<DonationEventDto>(updatedDonationEvent);
                    
                    return new ApiResponse<DonationEventDto>(
                        donationEventResultDto,
                        "Donor health check completed - Not eligible for donation");
                }
                
                // Xử lý trường hợp nhóm máu khác với hồ sơ
                if (healthCheckDto.VerifiedBloodGroupId.HasValue && 
                    healthCheckDto.VerifiedBloodGroupId.Value != donationEvent.BloodGroupId)
                {
                    // Cập nhật nhóm máu trong sự kiện hiến máu
                    donationEvent.BloodGroupId = healthCheckDto.VerifiedBloodGroupId.Value;
                    
                    // Cập nhật nhóm máu trong hồ sơ người hiến máu
                    if (donationEvent.DonorId.HasValue)
                    {
                        var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(donationEvent.DonorId.Value);
                        if (donor != null)
                        {
                            donor.BloodGroupId = healthCheckDto.VerifiedBloodGroupId.Value;
                            _unitOfWork.DonorProfiles.Update(donor);
                            
                            // Gửi thông báo về việc cập nhật nhóm máu
                            if (donor.UserId != Guid.Empty)
                            {
                                await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                                {
                                    UserId = donor.UserId,
                                    Title = "Blood Group Updated",
                                    Type = "BloodGroupUpdate",
                                    Message = "Your blood group has been updated based on today's testing."
                                });
                            }
                        }
                    }
                }
                
                // Cập nhật trạng thái
                donationEvent.Status = "HealthCheckPassed";
                
                _unitOfWork.DonationEvents.Update(donationEvent);
                await _unitOfWork.CompleteAsync();
                
                // Update related blood request status when health check passes
                if (donationEvent.RequestType == "Appointment" && donationEvent.RequestId.HasValue)
                {
                    var appointment = await _unitOfWork.DonationAppointmentRequests.GetByIdAsync(donationEvent.RequestId.Value);
                    if (appointment?.RelatedBloodRequestId.HasValue == true)
                    {
                        await _bloodRequestService.UpdateBloodRequestStatusWithNotesAsync(
                            appointment.RelatedBloodRequestId.Value, 
                            "DonationInProgress", 
                            "Health check thành công. Donor đủ điều kiện hiến máu và sẵn sàng bắt đầu quá trình hiến máu.");
                    }
                }
                
                // Gửi thông báo
                if (donationEvent.DonorId.HasValue)
                {
                    var donor = donationEvent.DonorProfile;
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Title = "Health Check Passed",
                            Type = "HealthCheckPassed",
                            Message = "You have passed the health check and are eligible to donate blood today."
                        });
                    }
                }
                
                // Send real-time notification for passed health check
                await _realTimeNotificationService.NotifyDonationEventStatusChange(
                    donationEvent.Id, "HealthCheckPassed", "Health check passed - ready for donation");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                var resultDonationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                var resultDto = _mapper.Map<DonationEventDto>(resultDonationEvent);
                
                return new ApiResponse<DonationEventDto>(
                    resultDto,
                    "Donor health check completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while performing health check: {DonationEventId}", healthCheckDto.DonationEventId);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while performing health check");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> StartDonationProcessAsync(StartDonationDto startDto)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(startDto.DonationEventId);
                if (donationEvent == null || donationEvent.DeletedTime != null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Donation event not found");
                }
                
                // Kiểm tra trạng thái hiện tại
                if (donationEvent.Status != "HealthCheckPassed")
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.BadRequest,
                        "Donation event must be in 'HealthCheckPassed' status to start donation process");
                }
                
                // Cập nhật trạng thái
                donationEvent.Status = "InProgress";
                donationEvent.DonationStartTime = DateTimeOffset.UtcNow;
                if (!string.IsNullOrEmpty(startDto.Notes))
                {
                    donationEvent.Notes = (donationEvent.Notes ?? "") + "\n" + startDto.Notes;
                }
                donationEvent.LastUpdatedTime = DateTimeOffset.UtcNow;
                
                _unitOfWork.DonationEvents.Update(donationEvent);
                await _unitOfWork.CompleteAsync();
                
                // Gửi thông báo
                if (donationEvent.DonorId.HasValue)
                {
                    var donor = donationEvent.DonorProfile;
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Title = "Donation Process Started",
                            Type = "DonationStarted",
                            Message = "Your blood donation process has started. Thank you for your generosity."
                        });
                    }
                }
                
                // Send real-time notification for donation start
                await _realTimeNotificationService.NotifyDonationEventStatusChange(
                    donationEvent.Id, "InProgress", "Blood donation process started");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                var updatedDonationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                var donationEventResultDto = _mapper.Map<DonationEventDto>(updatedDonationEvent);
                
                return new ApiResponse<DonationEventDto>(
                    donationEventResultDto,
                    "Donation process started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while starting donation process: {DonationEventId}", startDto.DonationEventId);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while starting donation process");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> RecordDonationComplicationAsync(DonationComplicationDto complicationDto)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(complicationDto.DonationEventId);
                if (donationEvent == null || donationEvent.DeletedTime != null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Donation event not found");
                }
                
                // Kiểm tra trạng thái hiện tại
                if (donationEvent.Status != "InProgress")
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.BadRequest,
                        "Donation event must be in 'InProgress' status to record complications");
                }
                
                // Cập nhật thông tin biến chứng
                donationEvent.Status = "Incomplete";
                donationEvent.StatusDescription = $"Donation interrupted due to {complicationDto.ComplicationType}";
                donationEvent.ComplicationType = complicationDto.ComplicationType;
                donationEvent.ComplicationDetails = complicationDto.Description;
                donationEvent.ActionTaken = complicationDto.ActionTaken;
                donationEvent.LastUpdatedTime = DateTimeOffset.UtcNow;
                
                // Cập nhật lượng máu đã thu được (nếu có)
                if (complicationDto.CollectedAmount.HasValue)
                {
                    donationEvent.QuantityDonated = complicationDto.CollectedAmount.Value;
                    donationEvent.IsUsable = complicationDto.IsUsable;
                }
                
                _unitOfWork.DonationEvents.Update(donationEvent);
                
                // Cập nhật lịch hẹn nếu có
                if (donationEvent.RequestType == "Appointment" && donationEvent.RequestId.HasValue)
                {
                    var appointment = await _unitOfWork.DonationAppointmentRequests.GetByIdAsync(donationEvent.RequestId.Value);
                    if (appointment != null)
                    {
                        appointment.Status = "Incomplete";
                        appointment.Notes = (appointment.Notes ?? "") + "\n" + $"Donation incomplete due to {complicationDto.ComplicationType}";
                        _unitOfWork.DonationAppointmentRequests.Update(appointment);
                        
                        // Update related blood request status when complication occurs
                        if (appointment.RelatedBloodRequestId.HasValue)
                        {
                            var isUsableNote = complicationDto.IsUsable ? "có thể sử dụng" : "không thể sử dụng";
                            await _bloodRequestService.UpdateBloodRequestStatusWithNotesAsync(
                                appointment.RelatedBloodRequestId.Value, 
                                "Processing", 
                                $"Donation bị gián đoạn do biến chứng: {complicationDto.ComplicationType}. " +
                                $"Lượng máu thu được: {complicationDto.CollectedAmount?.ToString() ?? "0"}ml ({isUsableNote}). " +
                                $"Cần tìm donor khác để bù đắp.");
                        }
                    }
                }
                
                await _unitOfWork.CompleteAsync();
                
                // Gửi thông báo
                if (donationEvent.DonorId.HasValue)
                {
                    var donor = donationEvent.DonorProfile;
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Title = "Donation Complication",
                            Type = "DonationComplication",
                            Message = $"Your donation was interrupted due to {complicationDto.ComplicationType}. Please take care and follow the post-donation instructions provided by our staff."
                        });
                    }
                }
                
                // Send real-time notification for complication
                await _realTimeNotificationService.NotifyDonationEventStatusChange(
                    donationEvent.Id, "Incomplete", $"Donation complication: {complicationDto.ComplicationType}");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                var updatedDonationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                var donationEventResultDto = _mapper.Map<DonationEventDto>(updatedDonationEvent);
                
                return new ApiResponse<DonationEventDto>(
                    donationEventResultDto,
                    "Donation complication recorded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while recording donation complication: {DonationEventId}", 
                    complicationDto.DonationEventId);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while recording donation complication");
            }
        }

        public async Task<ApiResponse<DonationEventDto>> CompleteDonationAsync(CompleteDonationDto completionDto)
        {
            try
            {
                var donationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(completionDto.DonationEventId);
                if (donationEvent == null || donationEvent.DeletedTime != null)
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.NotFound,
                        "Donation event not found");
                }

                // Kiểm tra trạng thái hiện tại
                if (donationEvent.Status != "InProgress")
                {
                    return new ApiResponse<DonationEventDto>(
                        HttpStatusCode.BadRequest,
                        "Donation event must be in 'InProgress' status to complete donation");
                }

                // Cập nhật thông tin hiến máu
                donationEvent.Status = "Completed";
                donationEvent.DonationDate = completionDto.DonationDate;
                donationEvent.QuantityDonated = completionDto.QuantityDonated;
                donationEvent.QuantityUnits = (int)Math.Round(completionDto.QuantityUnits);
                if (!string.IsNullOrEmpty(completionDto.Notes))
                {
                    donationEvent.Notes = (donationEvent.Notes ?? "") + "\n" + completionDto.Notes;
                }
                donationEvent.CompletedTime = DateTimeOffset.UtcNow;
                donationEvent.LastUpdatedTime = DateTimeOffset.UtcNow;
                donationEvent.IsUsable = true;

                _unitOfWork.DonationEvents.Update(donationEvent);

                // Lấy tên donor
                var donorName = "Unknown Donor";
                if (donationEvent.DonorId.HasValue)
                {
                    var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(donationEvent.DonorId.Value);
                    if (donor != null)
                    {
                        donor.LastDonationDate = completionDto.DonationDate;
                        donor.TotalDonations = donor.TotalDonations + 1;
                        donor.NextAvailableDonationDate = await CalculateNextAvailableDonationDateAsync(
                             completionDto.DonationDate, donationEvent.ComponentTypeId);
                        _unitOfWork.DonorProfiles.Update(donor);

                        donorName = $"{donor.User?.FirstName} {donor.User?.LastName}".Trim();
                        if (string.IsNullOrEmpty(donorName))
                        {
                            donorName = $"Donor ID: {donor.Id}";
                        }
                    }
                }

                // Cập nhật lịch hẹn nếu có
                if (donationEvent.RequestType == "Appointment" && donationEvent.RequestId.HasValue)
                {
                    var appointment = await _unitOfWork.DonationAppointmentRequests.GetByIdAsync(donationEvent.RequestId.Value);
                    if (appointment != null)
                    {
                        appointment.Status = "Completed";
                        appointment.CompletedTime = DateTimeOffset.UtcNow;
                        _unitOfWork.DonationAppointmentRequests.Update(appointment);
                    }
                }

                await _unitOfWork.CompleteAsync();

                // Tạo bản ghi kho máu mới
                await _bloodInventoryService.CreateBloodInventoryAsync(new CreateBloodInventoryDto
                {
                    DonationEventId = donationEvent.Id,
                    BloodGroupId = donationEvent.BloodGroupId,
                    ComponentTypeId = donationEvent.ComponentTypeId,
                    QuantityUnits = (int)completionDto.QuantityUnits,
                    Status = "Available",
                    InventorySource = "Donation",
                    ExpirationDate = await CalculateExpirationDateAsync(completionDto.DonationDate, donationEvent.ComponentTypeId)
                });

                _logger.LogInformation("🩸 New blood inventory created: {Units} units of {BloodGroup} {ComponentType}",
                    completionDto.QuantityUnits,
                    donationEvent.BloodGroup?.GroupName ?? "Unknown",
                    donationEvent.ComponentType?.Name ?? "Unknown");

                // 🔥 SIMPLE: Check và record contribution cho blood request liên quan (nếu có)
                if (donationEvent.RequestType == "Appointment" && donationEvent.RequestId.HasValue)
                {
                    var appointment = await _unitOfWork.DonationAppointmentRequests.GetByIdAsync(donationEvent.RequestId.Value);
                    if (appointment?.RelatedBloodRequestId.HasValue == true)
                    {
                        await RecordContributionToBloodRequestAsync(
                            appointment.RelatedBloodRequestId.Value,
                            (int)completionDto.QuantityUnits,
                            donorName,
                            donationEvent.BloodGroup?.GroupName ?? "Unknown",
                            donationEvent.ComponentType?.Name ?? "Unknown"
                        );
                    }
                }

                // Simple auto-fulfill other requests
                await SimpleAutoFulfillAsync(donationEvent.BloodGroupId, donationEvent.ComponentTypeId);

                // Gửi thông báo
                if (donationEvent.DonorId.HasValue)
                {
                    var donor = donationEvent.DonorProfile;
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Title = "Donation Completed Successfully",
                            Type = "DonationCompleted",
                            Message = "Thank you for your donation! Your blood will help save lives."
                        });

                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Title = "Post-Donation Care Reminder",
                            Type = "PostDonationCare",
                            Message = "Remember to rest and stay hydrated for the next 24 hours. Avoid heavy lifting and strenuous activities."
                        });

                        if (donor.NextAvailableDonationDate.HasValue)
                        {
                            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                            {
                                UserId = donor.UserId,
                                Title = "Next Donation Date",
                                Type = "NextDonationReminder",
                                Message = $"You will be eligible to donate blood again on {donor.NextAvailableDonationDate.Value:D}. We look forward to seeing you again!"
                            });
                        }
                    }
                }

                // Send real-time notification for completed donation
                await _realTimeNotificationService.NotifyDonationEventStatusChange(
                    donationEvent.Id, "Completed", "Blood donation completed successfully");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                await _realTimeNotificationService.UpdateInventoryDashboard();

                var updatedDonationEvent = await _unitOfWork.DonationEvents.GetByIdWithDetailsAsync(donationEvent.Id);
                var donationEventResultDto = _mapper.Map<DonationEventDto>(updatedDonationEvent);

                return new ApiResponse<DonationEventDto>(
                    donationEventResultDto,
                    "Donation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while completing donation: {DonationEventId}",
                    completionDto.DonationEventId);
                return new ApiResponse<DonationEventDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while completing donation");
            }
        }

        // 🔥 NEW Helper: Record contribution đơn giản
        private async Task RecordContributionToBloodRequestAsync(
            Guid requestId,
            int unitsContributed,
            string donorName,
            string bloodGroupName,
            string componentTypeName)
        {
            try
            {
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
                if (bloodRequest == null || bloodRequest.Status == "Fulfilled") return;

                // Parse current fulfilled từ medical notes
                int currentFulfilled = ExtractFulfilledUnitsFromNotes(bloodRequest.MedicalNotes);
                int newTotal = currentFulfilled + unitsContributed;
                int remaining = bloodRequest.QuantityUnits - newTotal;

                var vietnamTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

                // Update status
                string newStatus = remaining <= 0 ? "Fulfilled" : "Processing";
                bloodRequest.Status = newStatus;
                bloodRequest.LastUpdatedTime = DateTimeOffset.UtcNow;

                if (newStatus == "Fulfilled")
                {
                    bloodRequest.FulfilledDate = DateTimeOffset.UtcNow;
                }

                // Add progress note
                string progressNote = remaining <= 0
                    ? $"<div style='margin: 10px 0; padding: 10px; background: #e8f5e8; border-left: 4px solid #28a745; border-radius: 4px;'>" +
                      $"<strong>[{vietnamTime:dd/MM/yyyy HH:mm}]</strong> " +
                      $"✅ <strong>HOÀN THÀNH</strong> yêu cầu!<br/>" +
                      $"Donor cuối: <strong>{donorName}</strong> hiến {unitsContributed} đơn vị {bloodGroupName} {componentTypeName}<br/>" +
                      $"Tổng thu thập: <strong>{newTotal}/{bloodRequest.QuantityUnits} đơn vị</strong>" +
                      $"</div>"
                    : $"<div style='margin: 10px 0; padding: 10px; background: #fff3cd; border-left: 4px solid #ffc107; border-radius: 4px;'>" +
                      $"<strong>[{vietnamTime:dd/MM/yyyy HH:mm}]</strong> " +
                      $"📊 <strong>TIẾN ĐỘ</strong> hiến máu<br/>" +
                      $"Donor: <strong>{donorName}</strong> hiến {unitsContributed} đơn vị {bloodGroupName} {componentTypeName}<br/>" +
                      $"Đã thu thập: <strong>{newTotal}/{bloodRequest.QuantityUnits} đơn vị</strong> - " +
                      $"<span style='color: #dc3545; font-weight: bold;'>Còn thiếu {remaining} đơn vị</span>" +
                      $"</div>";

                bloodRequest.MedicalNotes = (bloodRequest.MedicalNotes ?? "") + progressNote;

                _unitOfWork.BloodRequests.Update(bloodRequest);
                await _unitOfWork.CompleteAsync();

                _logger.LogInformation("✅ Recorded contribution: {DonorName} contributed {Units} units. Status: {Status} ({Fulfilled}/{Total})",
                    donorName, unitsContributed, newStatus, newTotal, bloodRequest.QuantityUnits);

                // Send notification
                string message = newStatus == "Fulfilled"
                    ? $"🎉 Request fulfilled! Total: {newTotal} units from multiple donors"
                    : $"📈 Progress: {newTotal}/{bloodRequest.QuantityUnits} units. Need {remaining} more units";

                await _realTimeNotificationService.NotifyBloodRequestStatusChange(requestId, newStatus, message);

                if (bloodRequest.IsEmergency)
                {
                    await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(requestId, newStatus, message);
                    await _realTimeNotificationService.UpdateEmergencyDashboard();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording contribution for request {RequestId}", requestId);
            }
        }

        // 🔥 Helper: Parse fulfilled units từ medical notes
        private int ExtractFulfilledUnitsFromNotes(string medicalNotes)
        {
            if (string.IsNullOrEmpty(medicalNotes)) return 0;

            try
            {
                // Pattern để tìm "Đã thu thập: 2/5 đơn vị" hoặc "Tổng thu thập: 3/5 đơn vị"
                var pattern = @"(?:Đã thu thập|Tổng thu thập):\s*<strong>(\d+)/\d+\s*đơn vị</strong>";
                var matches = System.Text.RegularExpressions.Regex.Matches(medicalNotes, pattern);

                if (matches.Count > 0)
                {
                    var lastMatch = matches[matches.Count - 1];
                    if (int.TryParse(lastMatch.Groups[1].Value, out int units))
                    {
                        return units;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing fulfilled units from medical notes");
            }

            return 0;
        }

        // 🔥 Simple auto-fulfill
        private async Task SimpleAutoFulfillAsync(Guid bloodGroupId, Guid componentTypeId)
        {
            try
            {
                // Tìm requests đang chờ với cùng blood type
                var pendingRequests = await _unitOfWork.BloodRequests.FindAsync(r =>
                    r.BloodGroupId == bloodGroupId &&
                    r.ComponentTypeId == componentTypeId &&
                    r.Status == "Processing" &&
                    r.IsActive &&
                    r.DeletedTime == null);

                foreach (var request in pendingRequests.OrderBy(r => r.IsEmergency ? 0 : 1).ThenBy(r => r.RequestDate))
                {
                    // Check inventory
                    var inventoryCheck = await _bloodRequestService.CheckInventoryForRequestAsync(request.Id);
                    if (inventoryCheck.Success && inventoryCheck.Data.HasSufficientInventory)
                    {
                        // Tìm staff để auto-fulfill
                        var staff = await _unitOfWork.Users.FindAsync(u => u.Role.RoleName == "Admin" || u.Role.RoleName == "Staff");
                        var staffUser = staff.FirstOrDefault();

                        if (staffUser != null)
                        {
                            await _bloodRequestService.FulfillBloodRequestFromInventoryAsync(request.Id,
                                new FulfillBloodRequestDto
                                {
                                    StaffId = staffUser.Id,
                                    Notes = "🤖 Tự động fulfill từ kho sau khi có máu mới"
                                });

                            _logger.LogInformation("✅ Auto-fulfilled request {RequestId}", request.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in simple auto-fulfill");
            }
        }

        #region Helper Methods

        private async Task SendDonationEventNotificationAsync(DonationEvent donationEvent, string message)
        {
            try
            {
                if (donationEvent.DonorId.HasValue)
                {
                    var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(donationEvent.DonorId.Value);
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Title = "Donation Event Update",
                            Type = "DonationEventUpdate",
                            Message = message
                        });
                    }
                }
                
                _logger.LogInformation("Donation event notification: {Message}", message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending donation event notification");
            }
        }

        private async Task SendStatusChangeNotificationAsync(DonationEvent donationEvent, string previousStatus = null)
        {
            try
            {
                string message = $"Blood donation event status updated to {donationEvent.Status}";
                if (!string.IsNullOrEmpty(donationEvent.StatusDescription))
                {
                    message += $": {donationEvent.StatusDescription}";
                }
                
                await SendDonationEventNotificationAsync(donationEvent, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending status change notification");
            }
        }

        private async Task UpdateRequestStatusAsync(Guid requestId, string requestType, string newStatus)
        {
            try
            {
                // Chỉ sử dụng BloodRequest
                var request = await _unitOfWork.BloodRequests.GetByIdAsync(requestId);
                if (request != null)
                {
                    request.Status = newStatus;
                    _unitOfWork.BloodRequests.Update(request);
                    await _unitOfWork.CompleteAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request status. RequestId: {RequestId}, RequestType: {RequestType}, NewStatus: {NewStatus}",
                    requestId, requestType, newStatus);
            }
        }

        private async Task<DateTimeOffset> CalculateNextAvailableDonationDateAsync(DateTimeOffset donationDate, Guid componentTypeId)
        {
            try
            {
                // Get the component type information from database
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(componentTypeId);

                if (componentType == null)
                {
                    _logger.LogWarning("Component type not found with ID: {ComponentTypeId}. Using default waiting period.", componentTypeId);
                    return donationDate.AddDays(90); // Default 3 months
                }

                // Define waiting periods for different blood components (in days)
                var waitingPeriods = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    // Whole blood donations
                    { "Whole Blood", 90 },          // 3 months
                    { "Máu toàn phần", 90 },        // 3 months (Vietnamese)
                    
                    // Red blood cells
                    { "Red Blood Cells", 90 },      // 3 months
                    { "Packed Red Blood Cells", 90 }, // 3 months
                    { "PRBC", 90 },                 // 3 months
                    { "RBC", 90 },                  // 3 months
                    { "Hồng cầu", 90 },             // 3 months (Vietnamese)
                    { "Khối hồng cầu", 90 },        // 3 months (Vietnamese)
                    
                    // Platelets - shorter waiting period
                    { "Platelets", 14 },            // 2 weeks
                    { "Platelet Concentrate", 14 }, // 2 weeks
                    { "Tiểu cầu", 14 },             // 2 weeks (Vietnamese)
                    { "Cô đặc tiểu cầu", 14 },      // 2 weeks (Vietnamese)
                    
                    // Plasma - shorter waiting period
                    { "Plasma", 28 },               // 4 weeks
                    { "Fresh Frozen Plasma", 28 },  // 4 weeks
                    { "FFP", 28 },                  // 4 weeks
                    { "Huyết tương", 28 },          // 4 weeks (Vietnamese)
                    { "Huyết tương đông lạnh", 28 }, // 4 weeks (Vietnamese)
                    
                    // Cryoprecipitate
                    { "Cryoprecipitate", 28 },      // 4 weeks
                    { "Cryo", 28 },                 // 4 weeks
                    { "Cryoprecipitate AHF", 28 },  // 4 weeks
                    { "Cặn lạnh", 28 },             // 4 weeks (Vietnamese)
                    
                    // Granulocytes - very short waiting period
                    { "Granulocytes", 7 },          // 1 week
                    { "Bạch cầu hạt", 7 },          // 1 week (Vietnamese)
                    
                    // Double red cells - longer waiting period
                    { "Double Red Cells", 180 },    // 6 months
                    { "2RBC", 180 },                // 6 months
                    { "Hồng cầu đôi", 180 },        // 6 months (Vietnamese)
                    
                    // Apheresis products
                    { "Apheresis Platelets", 7 },   // 1 week
                    { "Apheresis Plasma", 28 },     // 4 weeks
                    { "Tiểu cầu ly tách", 7 },      // 1 week (Vietnamese)
                    { "Huyết tương ly tách", 28 },  // 4 weeks (Vietnamese)
                };

                // Try to find the waiting period for the component type
                if (waitingPeriods.TryGetValue(componentType.Name, out int waitingPeriodInDays))
                {
                    _logger.LogInformation("Using waiting period {Days} days for component type {ComponentType}",
                        waitingPeriodInDays, componentType.Name);
                    return donationDate.AddDays(waitingPeriodInDays);
                }

                // If no specific waiting period found, use intelligent default based on component name patterns
                var componentNameLower = componentType.Name.ToLower();

                if (componentNameLower.Contains("platelet") || componentNameLower.Contains("tiểu cầu"))
                {
                    _logger.LogInformation("Using platelet waiting period (14 days) for component type {ComponentType}", componentType.Name);
                    return donationDate.AddDays(14);
                }
                else if (componentNameLower.Contains("plasma") || componentNameLower.Contains("huyết tương"))
                {
                    _logger.LogInformation("Using plasma waiting period (28 days) for component type {ComponentType}", componentType.Name);
                    return donationDate.AddDays(28);
                }
                else if (componentNameLower.Contains("red") || componentNameLower.Contains("rbc") || componentNameLower.Contains("hồng cầu"))
                {
                    _logger.LogInformation("Using red blood cell waiting period (90 days) for component type {ComponentType}", componentType.Name);
                    return donationDate.AddDays(90);
                }
                else if (componentNameLower.Contains("whole") || componentNameLower.Contains("toàn phần"))
                {
                    _logger.LogInformation("Using whole blood waiting period (90 days) for component type {ComponentType}", componentType.Name);
                    return donationDate.AddDays(90);
                }
                else if (componentNameLower.Contains("cryo") || componentNameLower.Contains("cặn lạnh"))
                {
                    _logger.LogInformation("Using cryoprecipitate waiting period (28 days) for component type {ComponentType}", componentType.Name);
                    return donationDate.AddDays(28);
                }
                else if (componentNameLower.Contains("granulocyte") || componentNameLower.Contains("bạch cầu"))
                {
                    _logger.LogInformation("Using granulocyte waiting period (7 days) for component type {ComponentType}", componentType.Name);
                    return donationDate.AddDays(7);
                }

                // Default fallback - whole blood waiting period
                _logger.LogInformation("Using default waiting period (90 days) for unknown component type {ComponentType}", componentType.Name);
                return donationDate.AddDays(90);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next available donation date for component type {ComponentTypeId}. Using default 90 days.", componentTypeId);
                return donationDate.AddDays(90); // Safe fallback
            }
        }

        /// <summary>
        /// Calculate the expiration date for blood inventory based on component type and collection date
        /// </summary>
        /// <param name="collectionDate">The date when the blood was collected</param>
        /// <param name="componentTypeId">The ID of the blood component type</param>
        /// <returns>The expiration date for the blood component</returns>
        private async Task<DateTimeOffset> CalculateExpirationDateAsync(DateTimeOffset collectionDate, Guid componentTypeId)
        {
            try
            {
                // Get the component type information from database
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(componentTypeId);

                if (componentType == null)
                {
                    _logger.LogWarning("Component type not found with ID: {ComponentTypeId}. Using default expiration period.", componentTypeId);
                    return collectionDate.AddDays(42); // Default 42 days for red blood cells
                }

                // Use ShelfLifeDays from the database if available
                if (componentType.ShelfLifeDays > 0)
                {
                    _logger.LogInformation("Using database shelf life {Days} days for component type {ComponentType}",
                        componentType.ShelfLifeDays, componentType.Name);
                    return collectionDate.AddDays(componentType.ShelfLifeDays);
                }

                // Define expiration periods for different blood components (in days)
                var expirationPeriods = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    // Whole blood
                    { "Whole Blood", 42 },          // 42 days at 1-6°C
                    { "Máu toàn phần", 42 },        // 42 days (Vietnamese)
                    
                    // Red blood cells
                    { "Red Blood Cells", 42 },      // 42 days at 1-6°C
                    { "Packed Red Blood Cells", 42 }, // 42 days at 1-6°C
                    { "PRBC", 42 },                 // 42 days at 1-6°C
                    { "RBC", 42 },                  // 42 days at 1-6°C
                    { "Hồng cầu", 42 },             // 42 days (Vietnamese)
                    { "Khối hồng cầu", 42 },        // 42 days (Vietnamese)
                    
                    // Platelets - very short shelf life
                    { "Platelets", 5 },             // 5 days at 20-24°C with agitation
                    { "Platelet Concentrate", 5 },  // 5 days at 20-24°C with agitation
                    { "Tiểu cầu", 5 },              // 5 days (Vietnamese)
                    { "Cô đặc tiểu cầu", 5 },       // 5 days (Vietnamese)
                    
                    // Fresh Frozen Plasma
                    { "Plasma", 365 },              // 1 year at -18°C or colder
                    { "Fresh Frozen Plasma", 365 }, // 1 year at -18°C or colder
                    { "FFP", 365 },                 // 1 year at -18°C or colder
                    { "Huyết tương", 365 },         // 1 year (Vietnamese)
                    { "Huyết tương đông lạnh", 365 }, // 1 year (Vietnamese)
                    
                    // Thawed Plasma
                    { "Thawed Plasma", 5 },         // 5 days at 1-6°C after thawing
                    { "Thawed FFP", 5 },            // 5 days at 1-6°C after thawing
                    { "Huyết tương rã đông", 5 },   // 5 days (Vietnamese)
                    
                    // Cryoprecipitate
                    { "Cryoprecipitate", 365 },     // 1 year at -18°C or colder
                    { "Cryo", 365 },                // 1 year at -18°C or colder
                    { "Cryoprecipitate AHF", 365 }, // 1 year at -18°C or colder
                    { "Cặn lạnh", 365 },            // 1 year (Vietnamese)
                    
                    // Thawed Cryoprecipitate
                    { "Thawed Cryoprecipitate", 1 }, // 6 hours at room temperature or 4 hours at 1-6°C
                    { "Thawed Cryo", 1 },           // 6 hours at room temperature or 4 hours at 1-6°C
                    { "Cặn lạnh rã đông", 1 },      // 6 hours (Vietnamese)
                    
                    // Granulocytes - very short shelf life
                    { "Granulocytes", 1 },          // 24 hours at 20-24°C
                    { "Bạch cầu hạt", 1 },          // 24 hours (Vietnamese)
                    
                    // Extended storage red cells
                    { "Extended Storage RBC", 63 }, // 63 days with special additive solutions
                    { "Extended Storage Red Cells", 63 }, // 63 days with special additive solutions
                    { "Hồng cầu bảo quản kéo dài", 63 }, // 63 days (Vietnamese)
                    
                    // Apheresis products
                    { "Apheresis Platelets", 5 },   // 5 days at 20-24°C with agitation
                    { "Apheresis Plasma", 365 },    // 1 year at -18°C or colder
                    { "Tiểu cầu ly tách", 5 },      // 5 days (Vietnamese)
                    { "Huyết tương ly tách", 365 }, // 1 year (Vietnamese)
                    
                    // Leukocyte-reduced products
                    { "Leukocyte-Reduced RBC", 42 }, // 42 days at 1-6°C
                    { "Leukocyte-Reduced Platelets", 5 }, // 5 days at 20-24°C with agitation
                    { "Hồng cầu loại bỏ bạch cầu", 42 }, // 42 days (Vietnamese)
                    { "Tiểu cầu loại bỏ bạch cầu", 5 }, // 5 days (Vietnamese)
                    
                    // Irradiated products - slightly shorter shelf life
                    { "Irradiated RBC", 28 },       // 28 days from irradiation date or original expiry, whichever is sooner
                    { "Irradiated Platelets", 5 },  // 5 days at 20-24°C with agitation
                    { "Hồng cầu chiếu xạ", 28 },    // 28 days (Vietnamese)
                    { "Tiểu cầu chiếu xạ", 5 },     // 5 days (Vietnamese)
                };

                // Try to find the expiration period for the component type
                if (expirationPeriods.TryGetValue(componentType.Name, out int expirationPeriodInDays))
                {
                    _logger.LogInformation("Using expiration period {Days} days for component type {ComponentType}",
                        expirationPeriodInDays, componentType.Name);
                    return collectionDate.AddDays(expirationPeriodInDays);
                }

                // If no specific expiration period found, use intelligent default based on component name patterns
                var componentNameLower = componentType.Name.ToLower();

                if (componentNameLower.Contains("platelet") || componentNameLower.Contains("tiểu cầu"))
                {
                    _logger.LogInformation("Using platelet expiration period (5 days) for component type {ComponentType}", componentType.Name);
                    return collectionDate.AddDays(5);
                }
                else if (componentNameLower.Contains("plasma") || componentNameLower.Contains("huyết tương"))
                {
                    // Check if it's thawed plasma
                    if (componentNameLower.Contains("thawed") || componentNameLower.Contains("rã đông"))
                    {
                        _logger.LogInformation("Using thawed plasma expiration period (5 days) for component type {ComponentType}", componentType.Name);
                        return collectionDate.AddDays(5);
                    }
                    else
                    {
                        _logger.LogInformation("Using frozen plasma expiration period (365 days) for component type {ComponentType}", componentType.Name);
                        return collectionDate.AddDays(365);
                    }
                }
                else if (componentNameLower.Contains("red") || componentNameLower.Contains("rbc") || componentNameLower.Contains("hồng cầu"))
                {
                    // Check if it's irradiated
                    if (componentNameLower.Contains("irradiated") || componentNameLower.Contains("chiếu xạ"))
                    {
                        _logger.LogInformation("Using irradiated red blood cell expiration period (28 days) for component type {ComponentType}", componentType.Name);
                        return collectionDate.AddDays(28);
                    }
                    else
                    {
                        _logger.LogInformation("Using red blood cell expiration period (42 days) for component type {ComponentType}", componentType.Name);
                        return collectionDate.AddDays(42);
                    }
                }
                else if (componentNameLower.Contains("whole") || componentNameLower.Contains("toàn phần"))
                {
                    _logger.LogInformation("Using whole blood expiration period (42 days) for component type {ComponentType}", componentType.Name);
                    return collectionDate.AddDays(42);
                }
                else if (componentNameLower.Contains("cryo") || componentNameLower.Contains("cặn lạnh"))
                {
                    // Check if it's thawed cryoprecipitate
                    if (componentNameLower.Contains("thawed") || componentNameLower.Contains("rã đông"))
                    {
                        _logger.LogInformation("Using thawed cryoprecipitate expiration period (1 day) for component type {ComponentType}", componentType.Name);
                        return collectionDate.AddDays(1);
                    }
                    else
                    {
                        _logger.LogInformation("Using frozen cryoprecipitate expiration period (365 days) for component type {ComponentType}", componentType.Name);
                        return collectionDate.AddDays(365);
                    }
                }
                else if (componentNameLower.Contains("granulocyte") || componentNameLower.Contains("bạch cầu"))
                {
                    _logger.LogInformation("Using granulocyte expiration period (1 day) for component type {ComponentType}", componentType.Name);
                    return collectionDate.AddDays(1);
                }

                // Default fallback - red blood cell expiration period
                _logger.LogInformation("Using default expiration period (42 days) for unknown component type {ComponentType}", componentType.Name);
                return collectionDate.AddDays(42);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating expiration date for component type {ComponentTypeId}. Using default 42 days.", componentTypeId);
                return collectionDate.AddDays(42); // Safe fallback
            }
        }

        // 🔥 NEW: Auto-fulfill specific blood request
        private async Task TryAutoFulfillBloodRequestAsync(Guid bloodRequestId)
        {
            try
            {
                _logger.LogInformation("Attempting auto-fulfill for blood request {RequestId}", bloodRequestId);
                
                // Get blood request details
                var bloodRequest = await _unitOfWork.BloodRequests.GetByIdAsync(bloodRequestId);
                if (bloodRequest == null)
                {
                    _logger.LogWarning("Blood request {RequestId} not found during auto-fulfill attempt", bloodRequestId);
                    return;
                }
                
                // Skip if request is already fulfilled or cancelled
                if (bloodRequest.Status == "Fulfilled" || bloodRequest.Status == "Cancelled")
                {
                    _logger.LogInformation("Blood request {RequestId} already in final status: {Status}", bloodRequestId, bloodRequest.Status);
                    return;
                }
                
                // Check if request can be fulfilled from inventory
                var inventoryCheck = await _bloodRequestService.CheckInventoryForRequestAsync(bloodRequestId);
                if (!inventoryCheck.Success)
                {
                    _logger.LogWarning("Failed to check inventory for blood request {RequestId}: {Error}", bloodRequestId, inventoryCheck.Message);
                    return;
                }
                
                if (inventoryCheck.Data.HasSufficientInventory)
                {
                    // Find a system staff user for auto-fulfill
                    var systemStaff = await _unitOfWork.Users.FindAsync(u => 
                        u.UserName == "system" || 
                        u.UserName.ToLower().Contains("admin") ||
                        u.Role.RoleName == "Admin");
                    
                    var staffUser = systemStaff.FirstOrDefault();
                    Guid staffId;
                    
                    if (staffUser != null)
                    {
                        staffId = staffUser.Id;
                    }
                    else
                    {
                        // Fallback: find any admin/staff user
                        var adminStaff = await _unitOfWork.Users.FindAsync(u => 
                            u.Role.RoleName == "Admin" || u.Role.RoleName == "Staff");
                        staffUser = adminStaff.FirstOrDefault();
                        
                        if (staffUser != null)
                        {
                            staffId = staffUser.Id;
                        }
                        else
                        {
                            _logger.LogWarning("No suitable staff user found for auto-fulfill of request {RequestId}", bloodRequestId);
                            return;
                        }
                    }
                    
                    // Attempt to fulfill the request
                    var fulfillResult = await _bloodRequestService.FulfillBloodRequestFromInventoryAsync(
                        bloodRequestId, 
                        new FulfillBloodRequestDto 
                        { 
                            StaffId = staffId,
                            Notes = $"🤖 AUTO-FULFILL: Tự động đáp ứng sau khi nhận được máu từ hiến máu. " +
                                   $"Xử lý bởi hệ thống vào {DateTimeOffset.UtcNow:dd/MM/yyyy HH:mm}"
                        });
                    
                    if (fulfillResult.Success)
                    {
                        _logger.LogInformation("✅ Successfully auto-fulfilled blood request {RequestId} with {Units} units", 
                            bloodRequestId, bloodRequest.QuantityUnits);
                        
                        // Send real-time notification about auto-fulfill
                        await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                            bloodRequestId, 
                            "Fulfilled", 
                            $"🤖 Auto-fulfilled: Request automatically fulfilled from newly donated blood inventory");
                        
                        if (bloodRequest.IsEmergency)
                        {
                            await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                                bloodRequestId, 
                                "Fulfilled", 
                                "🚨 Emergency request auto-fulfilled from fresh donation!");
                            await _realTimeNotificationService.UpdateEmergencyDashboard();
                        }
                    }
                    else
                    {
                        _logger.LogWarning("❌ Failed to auto-fulfill blood request {RequestId}: {Error}", 
                            bloodRequestId, fulfillResult.Message);
                        
                        // Update request with failure note
                        await _bloodRequestService.UpdateBloodRequestStatusWithNotesAsync(
                            bloodRequestId, 
                            "Processing",
                            $"🤖 AUTO-FULFILL FAILED: {fulfillResult.Message}. Cần xử lý thủ công.");
                    }
                }
                else
                {
                    var availableUnits = inventoryCheck.Data.AvailableUnits;
                    var requiredUnits = inventoryCheck.Data.RequestedUnits;
                    
                    _logger.LogInformation("📦 Blood request {RequestId} cannot be auto-fulfilled - insufficient inventory (Available: {Available}, Required: {Required})", 
                        bloodRequestId, availableUnits, requiredUnits);
                    
                    // Update request status with partial inventory info
                    await _bloodRequestService.UpdateBloodRequestStatusWithNotesAsync(
                        bloodRequestId, 
                        "Processing",
                        $"🤖 AUTO-CHECK: Hiện có {availableUnits}/{requiredUnits} đơn vị trong kho. " +
                        $"Vẫn thiếu {requiredUnits - availableUnits} đơn vị để đáp ứng yêu cầu.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error during auto-fulfill attempt for blood request {RequestId}", bloodRequestId);
                
                try
                {
                    // Update request with error info
                    await _bloodRequestService.UpdateBloodRequestStatusWithNotesAsync(
                        bloodRequestId, 
                        "Processing",
                        $"🤖 AUTO-FULFILL ERROR: {ex.Message}. Cần kiểm tra và xử lý thủ công.");
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Failed to update request status after auto-fulfill error");
                }
            }
        }

        // 🔥 NEW: Auto-fulfill pending requests with matching blood type
        private async Task AutoFulfillPendingRequestsAsync(Guid bloodGroupId, Guid componentTypeId)
        {
            try
            {
                _logger.LogInformation("🔍 Looking for pending requests to auto-fulfill. BloodGroup: {BloodGroupId}, ComponentType: {ComponentTypeId}", 
                    bloodGroupId, componentTypeId);
                
                // Get blood group and component type names for logging
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodGroupId);
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(componentTypeId);
                var bloodGroupName = bloodGroup?.GroupName ?? "Unknown";
                var componentTypeName = componentType?.Name ?? "Unknown";
                
                _logger.LogInformation("🩸 Searching for {BloodGroup} {ComponentType} requests", bloodGroupName, componentTypeName);
                
                // Find pending requests that match the blood type from the donation
                var pendingRequests = await _unitOfWork.BloodRequests.FindAsync(r =>
                    r.BloodGroupId == bloodGroupId &&
                    r.ComponentTypeId == componentTypeId &&
                    (r.Status == "Pending" || r.Status == "Processing" || r.Status == "AwaitingDonation" || r.Status == "DonorConfirmed") &&
                    r.IsActive &&
                    r.DeletedTime == null);
                
                var requestsList = pendingRequests
                    .OrderBy(r => r.IsEmergency ? 0 : 1) // Emergency requests first
                    .ThenBy(r => r.RequestDate) // Then by request date (oldest first)
                    .ToList();
                
                if (!requestsList.Any())
                {
                    _logger.LogInformation("✅ No pending requests found for {BloodGroup} {ComponentType}", bloodGroupName, componentTypeName);
                    return;
                }
                
                _logger.LogInformation("📋 Found {Count} pending requests for {BloodGroup} {ComponentType}. Processing in priority order...", 
                    requestsList.Count, bloodGroupName, componentTypeName);
                
                int fulfilledCount = 0;
                int attemptedCount = 0;
                
                foreach (var request in requestsList)
                {
                    attemptedCount++;
                    var requestType = request.IsEmergency ? "🚨 EMERGENCY" : "📝 REGULAR";
                    var requestAge = (DateTimeOffset.UtcNow - request.RequestDate).TotalHours;
                    
                    _logger.LogInformation("🔄 Processing {RequestType} request {RequestId} (Age: {Age:F1} hours, Units: {Units})", 
                        requestType, request.Id, requestAge, request.QuantityUnits);
                    
                    var statusBeforeAttempt = request.Status;
                    await TryAutoFulfillBloodRequestAsync(request.Id);
                    
                    // Check if request was fulfilled
                    var updatedRequest = await _unitOfWork.BloodRequests.GetByIdAsync(request.Id);
                    if (updatedRequest?.Status == "Fulfilled")
                    {
                        fulfilledCount++;
                        _logger.LogInformation("✅ Successfully fulfilled {RequestType} request {RequestId}", requestType, request.Id);
                    }
                    else
                    {
                        _logger.LogInformation("⏳ Request {RequestId} not fulfilled - Status: {Status}", request.Id, updatedRequest?.Status);
                    }
                    
                    // Add a small delay between auto-fulfill attempts to avoid overwhelming the system
                    await Task.Delay(100);
                }
                
                // Log summary
                _logger.LogInformation("📊 AUTO-FULFILL SUMMARY for {BloodGroup} {ComponentType}: " +
                    "Attempted: {Attempted}, Fulfilled: {Fulfilled}, Remaining: {Remaining}", 
                    bloodGroupName, componentTypeName, attemptedCount, fulfilledCount, attemptedCount - fulfilledCount);
                
                // Send summary notification if any requests were fulfilled
                if (fulfilledCount > 0)
                {
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                    await _realTimeNotificationService.UpdateInventoryDashboard();
                    
                    _logger.LogInformation("🎉 Auto-fulfill process completed successfully! {Count} requests fulfilled for {BloodGroup} {ComponentType}", 
                        fulfilledCount, bloodGroupName, componentTypeName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error during auto-fulfill process for blood group {BloodGroupId} and component {ComponentTypeId}", 
                    bloodGroupId, componentTypeId);
                
                // Don't throw the exception as this is a background process
                // Log the error and continue with other operations
            }
        }

        #endregion

        #region Enhanced Helper Methods

        /// <summary>
        /// Get comprehensive inventory status for auto-fulfill decision making
        /// </summary>
        private async Task<(int totalAvailable, List<(Guid requestId, int unitsNeeded, bool isEmergency, double ageHours)> pendingRequests)> 
            GetInventoryAndPendingRequestsAsync(Guid bloodGroupId, Guid componentTypeId)
        {
            try
            {
                // Get available inventory
                var availableInventory = await _unitOfWork.BloodInventories.FindAsync(i =>
                    i.BloodGroupId == bloodGroupId &&
                    i.ComponentTypeId == componentTypeId &&
                    i.Status == "Available" &&
                    i.ExpirationDate > DateTimeOffset.UtcNow);
                
                var totalAvailable = availableInventory.Sum(i => i.QuantityUnits);
                
                // Get pending requests
                var pendingRequests = await _unitOfWork.BloodRequests.FindAsync(r =>
                    r.BloodGroupId == bloodGroupId &&
                    r.ComponentTypeId == componentTypeId &&
                    (r.Status == "Pending" || r.Status == "Processing" || r.Status == "AwaitingDonation" || r.Status == "DonorConfirmed") &&
                    r.IsActive &&
                    r.DeletedTime == null);
                
                var pendingRequestsInfo = pendingRequests
                    .Select(r => (
                        requestId: r.Id,
                        unitsNeeded: r.QuantityUnits,
                        isEmergency: r.IsEmergency,
                        ageHours: (DateTimeOffset.UtcNow - r.RequestDate).TotalHours
                    ))
                    .OrderBy(r => r.isEmergency ? 0 : 1) // Emergency first
                    .ThenBy(r => r.ageHours) // Then by age (oldest first)
                    .ToList();
                
                return (totalAvailable, pendingRequestsInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory and pending requests info");
                return (0, new List<(Guid, int, bool, double)>());
            }
        }

        /// <summary>
        /// Enhanced auto-fulfill with intelligent prioritization
        /// </summary>
        private async Task IntelligentAutoFulfillAsync(Guid bloodGroupId, Guid componentTypeId, int newInventoryUnits)
        {
            try
            {
                var bloodGroup = await _unitOfWork.BloodGroups.GetByIdAsync(bloodGroupId);
                var componentType = await _unitOfWork.ComponentTypes.GetByIdAsync(componentTypeId);
                var bloodGroupName = bloodGroup?.GroupName ?? "Unknown";
                var componentTypeName = componentType?.Name ?? "Unknown";
                
                _logger.LogInformation("🧠 INTELLIGENT AUTO-FULFILL started for {BloodGroup} {ComponentType} (+{NewUnits} units)", 
                    bloodGroupName, componentTypeName, newInventoryUnits);
                
                var (totalAvailable, pendingRequests) = await GetInventoryAndPendingRequestsAsync(bloodGroupId, componentTypeId);
                
                if (!pendingRequests.Any())
                {
                    _logger.LogInformation("✅ No pending requests found - inventory will be available for future needs");
                    return;
                }
                
                _logger.LogInformation("📊 INVENTORY STATUS: {Available} units available, {Requests} pending requests", 
                    totalAvailable, pendingRequests.Count);
                
                // Calculate fulfillment strategy
                int totalNeeded = pendingRequests.Sum(r => r.unitsNeeded);
                var emergencyRequests = pendingRequests.Where(r => r.isEmergency).ToList();
                var regularRequests = pendingRequests.Where(r => !r.isEmergency).ToList();
                
                _logger.LogInformation("📋 REQUESTS BREAKDOWN: {Emergency} emergency ({EmergencyUnits} units), {Regular} regular ({RegularUnits} units)", 
                    emergencyRequests.Count, emergencyRequests.Sum(r => r.unitsNeeded),
                    regularRequests.Count, regularRequests.Sum(r => r.unitsNeeded));
                
                if (totalAvailable >= totalNeeded)
                {
                    _logger.LogInformation("🎉 FULL FULFILLMENT POSSIBLE: Can fulfill all {Count} requests!", pendingRequests.Count);
                }
                else
                {
                    _logger.LogInformation("⚠️ PARTIAL FULFILLMENT: Can only fulfill {Available}/{Needed} units", totalAvailable, totalNeeded);
                }
                
                // Process requests in priority order
                int fulfilledCount = 0;
                int fulfilledUnits = 0;
                
                foreach (var request in pendingRequests)
                {
                    if (totalAvailable < request.unitsNeeded)
                    {
                        _logger.LogInformation("❌ Insufficient inventory for request {RequestId} ({Needed} units, {Available} available)", 
                            request.requestId, request.unitsNeeded, totalAvailable);
                        break;
                    }
                    
                    var requestType = request.isEmergency ? "🚨 EMERGENCY" : "📝 REGULAR";
                    _logger.LogInformation("🔄 Processing {Type} request {RequestId} (Age: {Age:F1}h, Units: {Units})", 
                        requestType, request.requestId, request.ageHours, request.unitsNeeded);
                    
                    await TryAutoFulfillBloodRequestAsync(request.requestId);
                    
                    // Check if fulfilled successfully
                    var updatedRequest = await _unitOfWork.BloodRequests.GetByIdAsync(request.requestId);
                    if (updatedRequest?.Status == "Fulfilled")
                    {
                        fulfilledCount++;
                        fulfilledUnits += request.unitsNeeded;
                        totalAvailable -= request.unitsNeeded; // Update available count
                        
                        _logger.LogInformation("✅ {Type} request {RequestId} fulfilled successfully", requestType, request.requestId);
                    }
                    
                    // Small delay between attempts
                    await Task.Delay(100);
                }
                
                // Final summary
                _logger.LogInformation("🎯 AUTO-FULFILL COMPLETED: {Fulfilled}/{Total} requests fulfilled, {Units} units dispensed", 
                    fulfilledCount, pendingRequests.Count, fulfilledUnits);
                
                if (fulfilledCount > 0)
                {
                    await _realTimeNotificationService.UpdateBloodRequestDashboard();
                    await _realTimeNotificationService.UpdateInventoryDashboard();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in intelligent auto-fulfill process");
            }
        }

        /// <summary>
        /// Generate auto-fulfill status report for debugging and monitoring
        /// </summary>
        public async Task<ApiResponse<object>> GetAutoFulfillStatusReportAsync(Guid? bloodGroupId = null, Guid? componentTypeId = null)
        {
            try
            {
                var report = new
                {
                    GeneratedAt = DateTimeOffset.UtcNow,
                    BloodGroups = new List<object>()
                };

                var bloodGroups = bloodGroupId.HasValue 
                    ? new[] { await _unitOfWork.BloodGroups.GetByIdAsync(bloodGroupId.Value) }.Where(bg => bg != null)
                    : await _unitOfWork.BloodGroups.GetAllAsync();

                foreach (var bloodGroup in bloodGroups)
                {
                    var componentTypes = componentTypeId.HasValue 
                        ? new[] { await _unitOfWork.ComponentTypes.GetByIdAsync(componentTypeId.Value) }.Where(ct => ct != null)
                        : await _unitOfWork.ComponentTypes.GetAllAsync();

                    var bloodGroupData = new
                    {
                        BloodGroupId = bloodGroup.Id,
                        BloodGroupName = bloodGroup.GroupName,
                        Components = new List<object>()
                    };

                    foreach (var component in componentTypes)
                    {
                        var (totalAvailable, pendingRequests) = await GetInventoryAndPendingRequestsAsync(bloodGroup.Id, component.Id);
                        
                        var componentData = new
                        {
                            ComponentTypeId = component.Id,
                            ComponentTypeName = component.Name,
                            Inventory = new
                            {
                                AvailableUnits = totalAvailable,
                                Status = totalAvailable > 0 ? "Available" : "Empty"
                            },
                            PendingRequests = new
                            {
                                Total = pendingRequests.Count,
                                Emergency = pendingRequests.Count(r => r.isEmergency),
                                Regular = pendingRequests.Count(r => !r.isEmergency),
                                TotalUnitsNeeded = pendingRequests.Sum(r => r.unitsNeeded),
                                CanFulfillAll = totalAvailable >= pendingRequests.Sum(r => r.unitsNeeded),
                                Details = pendingRequests.Select(r => new
                                {
                                    RequestId = r.requestId,
                                    UnitsNeeded = r.unitsNeeded,
                                    IsEmergency = r.isEmergency,
                                    AgeHours = Math.Round(r.ageHours, 1),
                                    CanBeFulfilled = totalAvailable >= r.unitsNeeded
                                }).ToList()
                            }
                        };

                        ((List<object>)bloodGroupData.Components).Add(componentData);
                    }

                    ((List<object>)report.BloodGroups).Add(bloodGroupData);
                }

                return new ApiResponse<object>(report, "Auto-fulfill status report generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating auto-fulfill status report");
                return new ApiResponse<object>(
                    HttpStatusCode.InternalServerError,
                    "Error generating auto-fulfill status report");
            }
        }

        #endregion
    }
}