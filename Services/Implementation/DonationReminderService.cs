using AutoMapper;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Repositories.Interface;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class DonationReminderService : IDonationReminderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<DonationReminderService> _logger;

        public DonationReminderService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService,
            IEmailService emailService,
            IMapper mapper,
            ILogger<DonationReminderService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _emailService = emailService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<int>> CheckAndSendRemindersAsync(int daysBeforeEligible = 7)
        {
            try
            {
                // L?y danh sách ng??i hi?n máu c?n ???c nh?c nh?
                var reminderSettings = await _unitOfWork.DonorReminderSettings.GetDonorsNeedingRemindersAsync(daysBeforeEligible);
                int remindersSent = 0;

                foreach (var settings in reminderSettings)
                {
                    try
                    {
                        if (settings.DonorProfile?.User == null)
                            continue;

                        var donor = settings.DonorProfile;
                        var user = donor.User;

                        if (!donor.NextAvailableDonationDate.HasValue)
                        {
                            _logger.LogWarning("Ng??i hi?n máu ID: {DonorId} không có ngày hi?n máu ti?p theo", settings.DonorProfileId);
                            continue;
                        }

                        // T?o thông báo
                        string reminderMessage = $"Xin chào {user.FirstName}, b?n s? ?? ?i?u ki?n hi?n máu vào ngày {donor.NextAvailableDonationDate?.ToString("dd/MM/yyyy")}. Hãy chu?n b? s?n sàng ?? tham gia hi?n máu và c?u s?ng nhi?u ng??i!";

                        // G?i thông báo trong ?ng d?ng
                        if (settings.InAppNotifications)
                        {
                            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                            {
                                UserId = user.Id,
                                Type = "DonationReminder",
                                Message = reminderMessage
                            });
                        }

                        // G?i email thông báo
                        if (settings.EmailNotifications && !string.IsNullOrEmpty(user.Email))
                        {
                            await _emailService.SendEmailAsync(
                                user.Email,
                                "Nh?c nh? v? th?i gian hi?n máu",
                                reminderMessage
                            );
                        }

                        // C?p nh?t th?i gian g?i nh?c nh? g?n nh?t
                        await _unitOfWork.DonorReminderSettings.UpdateLastReminderSentTimeAsync(settings.Id);
                        remindersSent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "L?i khi g?i nh?c nh? cho ng??i hi?n máu ID: {DonorId}", settings.DonorProfileId);
                        // Ti?p t?c v?i ng??i hi?n máu ti?p theo
                        continue;
                    }
                }

                await _unitOfWork.CompleteAsync();

                return new ApiResponse<int>(remindersSent, $"?ã g?i {remindersSent} nh?c nh? thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi ki?m tra và g?i nh?c nh? hi?n máu");
                return new ApiResponse<int>(HttpStatusCode.InternalServerError, "L?i khi g?i nh?c nh? hi?n máu");
            }
        }

        public async Task<ApiResponse<NotificationDto>> SendReminderAsync(Guid donorId, int daysBeforeEligible = 7)
        {
            try
            {
                // Ki?m tra ng??i hi?n máu
                var donor = await _unitOfWork.DonorProfiles.GetByIdWithDetailsAsync(donorId);
                if (donor == null || donor.DeletedTime != null)
                {
                    return new ApiResponse<NotificationDto>(
                        HttpStatusCode.NotFound,
                        "Không tìm th?y h? s? ng??i hi?n máu");
                }

                if (donor.User == null)
                {
                    return new ApiResponse<NotificationDto>(
                        HttpStatusCode.BadRequest,
                        "Ng??i hi?n máu không có thông tin ng??i dùng");
                }

                if (!donor.NextAvailableDonationDate.HasValue)
                {
                    return new ApiResponse<NotificationDto>(
                        HttpStatusCode.BadRequest,
                        "Ng??i hi?n máu ch?a có thông tin ngày hi?n máu ti?p theo");
                }

                // L?y cài ??t nh?c nh?, n?u ch?a có thì t?o m?i
                var reminderSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(donorId);
                if (reminderSettings == null)
                {
                    reminderSettings = new DonorReminderSettings
                    {
                        DonorProfileId = donorId,
                        EnableReminders = true,
                        DaysBeforeEligible = daysBeforeEligible,
                        EmailNotifications = true,
                        InAppNotifications = true
                    };
                    await _unitOfWork.DonorReminderSettings.AddAsync(reminderSettings);
                    await _unitOfWork.CompleteAsync();
                }

                // Ki?m tra xem ng??i hi?n máu có b?t nh?c nh? không
                if (!reminderSettings.EnableReminders)
                {
                    return new ApiResponse<NotificationDto>(
                        HttpStatusCode.BadRequest,
                        "Ng??i hi?n máu ?ã t?t tính n?ng nh?c nh?");
                }

                // Ki?m tra th?i gian g?i nh?c nh? g?n nh?t ?? tránh spam
                if (reminderSettings.LastReminderSentTime.HasValue)
                {
                    var daysSinceLastReminder = (DateTimeOffset.UtcNow - reminderSettings.LastReminderSentTime.Value).TotalDays;
                    if (daysSinceLastReminder < 1) // Ch? cho phép g?i 1 l?n/ngày
                    {
                        return new ApiResponse<NotificationDto>(
                            HttpStatusCode.BadRequest,
                            $"?ã g?i nh?c nh? cho ng??i hi?n máu này trong vòng 24 gi? qua. Vui lòng th? l?i sau.");
                    }
                }

                // T?o thông báo
                string reminderMessage = $"Xin chào {donor.User.FirstName}, b?n s? ?? ?i?u ki?n hi?n máu vào ngày {donor.NextAvailableDonationDate?.ToString("dd/MM/yyyy")}. Hãy chu?n b? s?n sàng ?? tham gia hi?n máu và c?u s?ng nhi?u ng??i!";

                // G?i thông báo trong ?ng d?ng n?u cài ??t cho phép
                ApiResponse<NotificationDto> notification;

                if (reminderSettings.InAppNotifications)
                {
                    notification = await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                    {
                        UserId = donor.User.Id,
                        Type = "DonationReminder",
                        Message = reminderMessage
                    });
                }
                else
                {
                    notification = new ApiResponse<NotificationDto>(new NotificationDto
                    {
                        UserId = donor.User.Id,
                        Type = "DonationReminder",
                        Message = reminderMessage,
                        CreatedTime = DateTimeOffset.UtcNow
                    }, "Thông báo ?ã ???c t?o nh?ng không g?i trong ?ng d?ng do cài ??t ng??i dùng");
                }

                // G?i email thông báo n?u có email và cài ??t cho phép
                if (reminderSettings.EmailNotifications && !string.IsNullOrEmpty(donor.User.Email))
                {
                    await _emailService.SendEmailAsync(
                        donor.User.Email,
                        "Nh?c nh? v? th?i gian hi?n máu",
                        reminderMessage
                    );

                    if (!reminderSettings.InAppNotifications)
                    {
                        _logger.LogInformation("?ã g?i email nh?c nh? cho ng??i hi?n máu ID: {DonorId} ({Email})",
                            donorId, donor.User.Email);
                    }
                }

                // C?p nh?t th?i gian g?i nh?c nh? g?n nh?t
                await _unitOfWork.DonorReminderSettings.UpdateLastReminderSentTimeAsync(reminderSettings.Id);
                await _unitOfWork.CompleteAsync();

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi g?i nh?c nh? cho ng??i hi?n máu ID: {DonorId}", donorId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, "L?i khi g?i nh?c nh? hi?n máu");
            }
        }

        public async Task<ApiResponse<DonorReminderSettingsDto>> GetReminderSettingsAsync(Guid donorId)
        {
            try
            {
                // Ki?m tra ng??i hi?n máu
                var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(donorId);
                if (donor == null || donor.DeletedTime != null)
                {
                    return new ApiResponse<DonorReminderSettingsDto>(
                        HttpStatusCode.NotFound,
                        "Không tìm th?y h? s? ng??i hi?n máu");
                }

                // L?y cài ??t nh?c nh?
                var reminderSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(donorId);

                // N?u ch?a có cài ??t, t?o cài ??t m?c ??nh
                if (reminderSettings == null)
                {
                    reminderSettings = new DonorReminderSettings
                    {
                        DonorProfileId = donorId,
                        EnableReminders = true,
                        DaysBeforeEligible = 7,
                        EmailNotifications = true,
                        InAppNotifications = true
                    };

                    await _unitOfWork.DonorReminderSettings.AddAsync(reminderSettings);
                    await _unitOfWork.CompleteAsync();

                    // L?y l?i cài ??t v?i thông tin ??y ??
                    reminderSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(donorId);
                }

                var settingsDto = new DonorReminderSettingsDto
                {
                    Id = reminderSettings.Id,
                    DonorProfileId = reminderSettings.DonorProfileId,
                    DonorName = reminderSettings.DonorProfile?.User != null ?
                        $"{reminderSettings.DonorProfile.User.FirstName} {reminderSettings.DonorProfile.User.LastName}" : "",
                    EnableReminders = reminderSettings.EnableReminders,
                    DaysBeforeEligible = reminderSettings.DaysBeforeEligible,
                    EmailNotifications = reminderSettings.EmailNotifications,
                    InAppNotifications = reminderSettings.InAppNotifications,
                    CreatedTime = reminderSettings.CreatedTime,
                    LastUpdatedTime = reminderSettings.LastUpdatedTime
                };

                return new ApiResponse<DonorReminderSettingsDto>(settingsDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi l?y cài ??t nh?c nh? cho ng??i hi?n máu ID: {DonorId}", donorId);
                return new ApiResponse<DonorReminderSettingsDto>(
                    HttpStatusCode.InternalServerError,
                    "L?i khi l?y cài ??t nh?c nh? hi?n máu");
            }
        }

        public async Task<ApiResponse<DonorReminderSettingsDto>> UpdateReminderSettingsAsync(UpdateDonorReminderSettingsDto reminderSettingsDto)
        {
            try
            {
                // Ki?m tra ng??i hi?n máu
                var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(reminderSettingsDto.DonorProfileId);
                if (donor == null || donor.DeletedTime != null)
                {
                    return new ApiResponse<DonorReminderSettingsDto>(
                        HttpStatusCode.NotFound,
                        "Không tìm th?y h? s? ng??i hi?n máu");
                }

                // L?y cài ??t nh?c nh?
                var reminderSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(reminderSettingsDto.DonorProfileId);

                // N?u ch?a có cài ??t, t?o m?i
                if (reminderSettings == null)
                {
                    reminderSettings = new DonorReminderSettings
                    {
                        DonorProfileId = reminderSettingsDto.DonorProfileId,
                        EnableReminders = reminderSettingsDto.EnableReminders,
                        DaysBeforeEligible = reminderSettingsDto.DaysBeforeEligible,
                        EmailNotifications = reminderSettingsDto.EmailNotifications,
                        InAppNotifications = reminderSettingsDto.InAppNotifications,
                        CreatedTime = DateTimeOffset.UtcNow
                    };

                    await _unitOfWork.DonorReminderSettings.AddAsync(reminderSettings);
                }
                else
                {
                    // C?p nh?t cài ??t hi?n có
                    reminderSettings.EnableReminders = reminderSettingsDto.EnableReminders;
                    reminderSettings.DaysBeforeEligible = reminderSettingsDto.DaysBeforeEligible;
                    reminderSettings.EmailNotifications = reminderSettingsDto.EmailNotifications;
                    reminderSettings.InAppNotifications = reminderSettingsDto.InAppNotifications;
                    reminderSettings.LastUpdatedTime = DateTimeOffset.UtcNow;

                    _unitOfWork.DonorReminderSettings.Update(reminderSettings);
                }

                await _unitOfWork.CompleteAsync();

                // L?y l?i cài ??t v?i thông tin ??y ??
                var updatedSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(reminderSettingsDto.DonorProfileId);

                var settingsDto = new DonorReminderSettingsDto
                {
                    Id = updatedSettings.Id,
                    DonorProfileId = updatedSettings.DonorProfileId,
                    DonorName = updatedSettings.DonorProfile?.User != null ?
                        $"{updatedSettings.DonorProfile.User.FirstName} {updatedSettings.DonorProfile.User.LastName}" : "",
                    EnableReminders = updatedSettings.EnableReminders,
                    DaysBeforeEligible = updatedSettings.DaysBeforeEligible,
                    EmailNotifications = updatedSettings.EmailNotifications,
                    InAppNotifications = updatedSettings.InAppNotifications,
                    CreatedTime = updatedSettings.CreatedTime,
                    LastUpdatedTime = updatedSettings.LastUpdatedTime
                };

                return new ApiResponse<DonorReminderSettingsDto>(
                    settingsDto,
                    "Cài ??t nh?c nh? ?ã ???c c?p nh?t thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "L?i khi c?p nh?t cài ??t nh?c nh? cho ng??i hi?n máu ID: {DonorId}",
                    reminderSettingsDto.DonorProfileId);
                return new ApiResponse<DonorReminderSettingsDto>(
                    HttpStatusCode.InternalServerError,
                    "L?i khi c?p nh?t cài ??t nh?c nh? hi?n máu");
            }
        }
    }
}