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
                // Get the list of donors needing reminders
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
                            _logger.LogWarning("Donor ID: {DonorId} does not have a next donation date", settings.DonorProfileId);
                            continue;
                        }

                        // Create notification message
                        string reminderMessage = $"Hello {user.FirstName}, you will be eligible to donate blood on {donor.NextAvailableDonationDate?.ToString("dd/MM/yyyy")}. Please be ready to donate and save lives!";

                        // Send in-app notification
                        if (settings.InAppNotifications)
                        {
                            await _notificationService.CreateNotificationAsync(new CreateNotificationDto
                            {
                                UserId = user.Id,
                                Type = "DonationReminder",
                                Message = reminderMessage
                            });
                        }

                        // Send email notification
                        if (settings.EmailNotifications && !string.IsNullOrEmpty(user.Email))
                        {
                            await _emailService.SendEmailAsync(
                                user.Email,
                                "Blood Donation Reminder",
                                reminderMessage
                            );
                        }

                        // Update last reminder sent time
                        await _unitOfWork.DonorReminderSettings.UpdateLastReminderSentTimeAsync(settings.Id);
                        remindersSent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error sending reminder to donor ID: {DonorId}", settings.DonorProfileId);
                        // Continue with next donor
                        continue;
                    }
                }

                await _unitOfWork.CompleteAsync();

                return new ApiResponse<int>(remindersSent, $"Successfully sent {remindersSent} reminders");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking and sending donation reminders");
                return new ApiResponse<int>(HttpStatusCode.InternalServerError, "Error sending donation reminders");
            }
        }

        public async Task<ApiResponse<NotificationDto>> SendReminderAsync(Guid donorId, int daysBeforeEligible = 7)
        {
            try
            {
                // Check donor
                var donor = await _unitOfWork.DonorProfiles.GetByIdWithDetailsAsync(donorId);
                if (donor == null || donor.DeletedTime != null)
                {
                    return new ApiResponse<NotificationDto>(
                        HttpStatusCode.NotFound,
                        "Donor profile not found");
                }

                if (donor.User == null)
                {
                    return new ApiResponse<NotificationDto>(
                        HttpStatusCode.BadRequest,
                        "Donor does not have user information");
                }

                if (!donor.NextAvailableDonationDate.HasValue)
                {
                    return new ApiResponse<NotificationDto>(
                        HttpStatusCode.BadRequest,
                        "Donor does not have next donation date information");
                }

                // Get reminder settings, create if not exists
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

                // Check if reminders are enabled
                if (!reminderSettings.EnableReminders)
                {
                    return new ApiResponse<NotificationDto>(
                        HttpStatusCode.BadRequest,
                        "Donor has disabled reminders");
                }

                // Check last reminder sent time to avoid spam
                if (reminderSettings.LastReminderSentTime.HasValue)
                {
                    var daysSinceLastReminder = (DateTimeOffset.UtcNow - reminderSettings.LastReminderSentTime.Value).TotalDays;
                    if (daysSinceLastReminder < 1) // Only allow once per day
                    {
                        return new ApiResponse<NotificationDto>(
                            HttpStatusCode.BadRequest,
                            $"Reminder has been sent to this donor within the last 24 hours. Please try again later.");
                    }
                }

                // Create notification message
                string reminderMessage = $"Hello {donor.User.FirstName}, you will be eligible to donate blood on {donor.NextAvailableDonationDate?.ToString("dd/MM/yyyy")}. Please be ready to donate and save lives!";

                // Send in-app notification if settings allow
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
                    }, "Notification created but not sent in-app due to user settings");
                }

                // Send email notification if email exists and settings allow
                if (reminderSettings.EmailNotifications && !string.IsNullOrEmpty(donor.User.Email))
                {
                    await _emailService.SendEmailAsync(
                        donor.User.Email,
                        "Blood Donation Reminder",
                        reminderMessage
                    );

                    if (!reminderSettings.InAppNotifications)
                    {
                        _logger.LogInformation("Email reminder sent to donor ID: {DonorId} ({Email})",
                            donorId, donor.User.Email);
                    }
                }

                // Update last reminder sent time
                await _unitOfWork.DonorReminderSettings.UpdateLastReminderSentTimeAsync(reminderSettings.Id);
                await _unitOfWork.CompleteAsync();

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder to donor ID: {DonorId}", donorId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, "Error sending donation reminder");
            }
        }

        public async Task<ApiResponse<DonorReminderSettingsDto>> CreateReminderSettingsAsync(CreateDonorReminderSettingsDto reminderSettingsDto)
        {
            try
            {
                // Check donor
                var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(reminderSettingsDto.DonorProfileId);
                if (donor == null || donor.DeletedTime != null)
                {
                    return new ApiResponse<DonorReminderSettingsDto>(
                        HttpStatusCode.NotFound,
                        "Donor profile not found");
                }

                // Check if reminder settings already exist for this donor
                var existingSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(reminderSettingsDto.DonorProfileId);
                if (existingSettings != null)
                {
                    return new ApiResponse<DonorReminderSettingsDto>(
                        HttpStatusCode.BadRequest,
                        "Reminder settings for this donor already exist. Please use the update API instead.");
                }

                // Create new reminder settings
                var reminderSettings = new DonorReminderSettings
                {
                    DonorProfileId = reminderSettingsDto.DonorProfileId,
                    EnableReminders = reminderSettingsDto.EnableReminders,
                    DaysBeforeEligible = reminderSettingsDto.DaysBeforeEligible,
                    EmailNotifications = reminderSettingsDto.EmailNotifications,
                    InAppNotifications = reminderSettingsDto.InAppNotifications,
                    CreatedTime = DateTimeOffset.UtcNow
                };

                await _unitOfWork.DonorReminderSettings.AddAsync(reminderSettings);
                await _unitOfWork.CompleteAsync();

                // Get the created settings with full details
                var createdSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(reminderSettingsDto.DonorProfileId);

                var settingsDto = new DonorReminderSettingsDto
                {
                    Id = createdSettings.Id,
                    DonorProfileId = createdSettings.DonorProfileId,
                    DonorName = createdSettings.DonorProfile?.User != null ?
                        $"{createdSettings.DonorProfile.User.FirstName} {createdSettings.DonorProfile.User.LastName}" : "",
                    EnableReminders = createdSettings.EnableReminders,
                    DaysBeforeEligible = createdSettings.DaysBeforeEligible,
                    EmailNotifications = createdSettings.EmailNotifications,
                    InAppNotifications = createdSettings.InAppNotifications,
                    CreatedTime = createdSettings.CreatedTime,
                    LastUpdatedTime = createdSettings.LastUpdatedTime
                };

                return new ApiResponse<DonorReminderSettingsDto>(
                    settingsDto,
                    "Reminder settings created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reminder settings for donor ID: {DonorId}",
                    reminderSettingsDto.DonorProfileId);
                return new ApiResponse<DonorReminderSettingsDto>(
                    HttpStatusCode.InternalServerError,
                    "Error creating donation reminder settings");
            }
        }

        public async Task<ApiResponse<DonorReminderSettingsDto>> GetReminderSettingsAsync(Guid donorId)
        {
            try
            {
                // Check donor
                var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(donorId);
                if (donor == null || donor.DeletedTime != null)
                {
                    return new ApiResponse<DonorReminderSettingsDto>(
                        HttpStatusCode.NotFound,
                        "Donor profile not found");
                }

                // Get reminder settings
                var reminderSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(donorId);

                // If no settings exist, create default settings
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

                    // Get the settings with full details
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
                _logger.LogError(ex, "Error getting reminder settings for donor ID: {DonorId}", donorId);
                return new ApiResponse<DonorReminderSettingsDto>(
                    HttpStatusCode.InternalServerError,
                    "Error getting donation reminder settings");
            }
        }

        public async Task<ApiResponse<DonorReminderSettingsDto>> UpdateReminderSettingsAsync(UpdateDonorReminderSettingsDto reminderSettingsDto)
        {
            try
            {
                // Check donor
                var donor = await _unitOfWork.DonorProfiles.GetByIdAsync(reminderSettingsDto.DonorProfileId);
                if (donor == null || donor.DeletedTime != null)
                {
                    return new ApiResponse<DonorReminderSettingsDto>(
                        HttpStatusCode.NotFound,
                        "Donor profile not found");
                }

                // Get reminder settings
                var reminderSettings = await _unitOfWork.DonorReminderSettings.GetByDonorProfileIdAsync(reminderSettingsDto.DonorProfileId);

                // If no settings exist, create new
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
                    // Update existing settings
                    reminderSettings.EnableReminders = reminderSettingsDto.EnableReminders;
                    reminderSettings.DaysBeforeEligible = reminderSettingsDto.DaysBeforeEligible;
                    reminderSettings.EmailNotifications = reminderSettingsDto.EmailNotifications;
                    reminderSettings.InAppNotifications = reminderSettingsDto.InAppNotifications;
                    reminderSettings.LastUpdatedTime = DateTimeOffset.UtcNow;

                    _unitOfWork.DonorReminderSettings.Update(reminderSettings);
                }

                await _unitOfWork.CompleteAsync();

                // Get the updated settings with full details
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
                    "Reminder settings updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reminder settings for donor ID: {DonorId}",
                    reminderSettingsDto.DonorProfileId);
                return new ApiResponse<DonorReminderSettingsDto>(
                    HttpStatusCode.InternalServerError,
                    "Error updating donation reminder settings");
            }
        }
    }
}