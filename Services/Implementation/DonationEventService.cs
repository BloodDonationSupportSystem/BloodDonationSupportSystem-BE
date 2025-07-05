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
                
                // Cập nhật thông tin người hiến máu
                if (donationEvent.DonorId.HasValue)
                {
                    var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(donationEvent.DonorId.Value);
                    if (donor != null)
                    {
                        donor.LastDonationDate = completionDto.DonationDate;
                        donor.TotalDonations = donor.TotalDonations + 1;
                        
                        // Tính ngày có thể hiến máu tiếp theo (mặc định 3 tháng)
                        donor.NextAvailableDonationDate = CalculateNextAvailableDonationDate(
                            completionDto.DonationDate, donationEvent.ComponentTypeId);
                        
                        _unitOfWork.DonorProfiles.Update(donor);
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
                    ExpirationDate = CalculateExpirationDate(completionDto.DonationDate, donationEvent.ComponentTypeId)
                });
                
                // Gửi thông báo
                if (donationEvent.DonorId.HasValue)
                {
                    var donor = donationEvent.DonorProfile;
                    if (donor != null && donor.UserId != Guid.Empty)
                    {
                        // Thông báo hoàn thành hiến máu
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Title = "Donation Completed Successfully",
                            Type = "DonationCompleted",
                            Message = "Thank you for your donation! Your blood will help save lives."
                        });
                        
                        // Thông báo chăm sóc sau hiến máu
                        await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                        {
                            UserId = donor.UserId,
                            Title = "Post-Donation Care Reminder",
                            Type = "PostDonationCare",
                            Message = "Remember to rest and stay hydrated for the next 24 hours. Avoid heavy lifting and strenuous activities."
                        });
                        
                        // Thông báo lịch hiến máu tiếp theo
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

        private DateTimeOffset CalculateNextAvailableDonationDate(DateTimeOffset donationDate, Guid componentTypeId)
        {
            // Mặc định là 3 tháng cho toàn phần
            int waitingPeriodInDays = 90;
            
            // Có thể điều chỉnh dựa trên loại thành phần máu
            // VD: Hiến tiểu cầu có thể hiến lại sau 2 tuần
            // Cần truy vấn thông tin thành phần máu để xác định chính xác
            
            return donationDate.AddDays(waitingPeriodInDays);
        }

        private DateTimeOffset CalculateExpirationDate(DateTimeOffset collectionDate, Guid componentTypeId)
        {
            // Mặc định là 42 ngày cho hồng cầu
            int expirationPeriodInDays = 42;
            
            // Có thể điều chỉnh dựa trên loại thành phần máu
            // VD: Tiểu cầu chỉ tồn tại 5 ngày, huyết tương đông lạnh có thể lên tới 1 năm
            // Cần truy vấn thông tin thành phần máu để xác định chính xác
            
            return collectionDate.AddDays(expirationPeriodInDays);
        }

        #endregion
    }
}