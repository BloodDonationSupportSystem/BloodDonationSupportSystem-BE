using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Services.Interface;
using Shared.Constants;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<NotificationDto>>> GetAllNotificationsAsync()
        {
            try
            {
                var notifications = await _unitOfWork.Notifications.FindAsync(n => n.DeletedTime == null);
                var notificationDtos = notifications.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<NotificationDto>>(notificationDtos)
                {
                    Message = $"Retrieved {notificationDtos.Count} notifications successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all notifications");
                return new ApiResponse<IEnumerable<NotificationDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(Guid id)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdWithUserAsync(id);

                if (notification == null || notification.DeletedTime != null)
                    return new ApiResponse<NotificationDto>(HttpStatusCode.NotFound, $"Notification with ID {id} not found");

                return new ApiResponse<NotificationDto>(MapToDto(notification));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification with ID: {Id}", id);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<NotificationDto>>> GetNotificationsByUserIdAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<NotificationDto>>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(userId);
                var notificationDtos = notifications.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<NotificationDto>>(notificationDtos)
                {
                    Message = $"Retrieved {notificationDtos.Count} notifications for user {userId} successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for user ID: {UserId}", userId);
                return new ApiResponse<IEnumerable<NotificationDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<NotificationDto>>> GetUnreadNotificationsByUserIdAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<NotificationDto>>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                var notifications = await _unitOfWork.Notifications.GetUnreadByUserIdAsync(userId);
                var notificationDtos = notifications.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<NotificationDto>>(notificationDtos)
                {
                    Message = $"Retrieved {notificationDtos.Count} unread notifications for user {userId} successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notifications for user ID: {UserId}", userId);
                return new ApiResponse<IEnumerable<NotificationDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<int>> GetUnreadCountByUserIdAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<int>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                var count = await _unitOfWork.Notifications.GetUnreadCountByUserIdAsync(userId);

                return new ApiResponse<int>(count)
                {
                    Message = $"User has {count} unread notifications"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unread notification count for user ID: {UserId}", userId);
                return new ApiResponse<int>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto notificationDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(notificationDto.UserId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {notificationDto.UserId} not found");
                }

                var notification = new Notification
                {
                    Title = notificationDto.Title,
                    Type = notificationDto.Type,
                    Message = notificationDto.Message,
                    IsRead = false,
                    UserId = notificationDto.UserId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Send email if user has email and it's an important notification
                if (!string.IsNullOrEmpty(user.Email) && ShouldSendEmail(notificationDto.Type))
                {
                    await SendNotificationEmailAsync(user, notification);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification for user ID: {UserId}", notificationDto.UserId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> UpdateNotificationAsync(Guid id, UpdateNotificationDto notificationDto)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(id);

                if (notification == null || notification.DeletedTime != null)
                    return new ApiResponse<NotificationDto>(HttpStatusCode.NotFound, $"Notification with ID {id} not found");

                notification.IsRead = notificationDto.IsRead;
                notification.LastUpdatedTime = DateTimeOffset.Now;

                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.CompleteAsync();

                var updatedNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(id);
                return new ApiResponse<NotificationDto>(MapToDto(updatedNotification), "Notification updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification with ID: {Id}", id);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> MarkNotificationAsReadAsync(Guid id)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(id);

                if (notification == null || notification.DeletedTime != null)
                    return new ApiResponse<NotificationDto>(HttpStatusCode.NotFound, $"Notification with ID {id} not found");

                if (notification.IsRead)
                {
                    var alreadyReadNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(id);
                    return new ApiResponse<NotificationDto>(MapToDto(alreadyReadNotification), "Notification was already marked as read");
                }

                notification.IsRead = true;
                notification.LastUpdatedTime = DateTimeOffset.Now;

                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.CompleteAsync();

                var updatedNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(id);
                return new ApiResponse<NotificationDto>(MapToDto(updatedNotification), "Notification marked as read successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read with ID: {Id}", id);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteNotificationAsync(Guid id)
        {
            try
            {
                var notification = await _unitOfWork.Notifications.GetByIdAsync(id);

                if (notification == null || notification.DeletedTime != null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Notification with ID {id} not found");

                notification.DeletedTime = DateTimeOffset.Now;
                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification with ID: {Id}", id);
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> MarkAllAsReadForUserAsync(Guid userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                await _unitOfWork.Notifications.MarkAllAsReadForUserAsync(userId);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse
                {
                    Message = "All notifications marked as read successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user ID: {UserId}", userId);
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PagedApiResponse<NotificationDto>> GetPagedNotificationsAsync(NotificationParameters parameters)
        {
            try
            {
                var (notifications, totalCount) = await _unitOfWork.Notifications.GetPagedNotificationsAsync(parameters);
                var notificationDtos = notifications.Select(MapToDto).ToList();

                return new PagedApiResponse<NotificationDto>(
                    notificationDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged notifications");
                return new PagedApiResponse<NotificationDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        public async Task<PagedApiResponse<NotificationDto>> GetPagedNotificationsByUserIdAsync(Guid userId, NotificationParameters parameters)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new PagedApiResponse<NotificationDto>
                    {
                        Success = false,
                        StatusCode = HttpStatusCode.BadRequest,
                        Message = $"User with ID {userId} not found"
                    };
                }

                parameters ??= new NotificationParameters();

                if (string.IsNullOrEmpty(parameters.SortBy))
                {
                    parameters.SortBy = "createdtime";
                    parameters.SortAscending = false;
                }

                var (notifications, totalCount) = await _unitOfWork.Notifications.GetPagedNotificationsByUserIdAsync(userId, parameters);
                var notificationDtos = notifications.Select(MapToDto).ToList();

                string filterInfo = "";
                if (!string.IsNullOrEmpty(parameters.Type) || parameters.IsRead.HasValue)
                {
                    var filters = new List<string>();
                    if (!string.IsNullOrEmpty(parameters.Type))
                        filters.Add($"type: {parameters.Type}");
                    if (parameters.IsRead.HasValue)
                        filters.Add($"read status: {(parameters.IsRead.Value ? "read" : "unread")}");
                    filterInfo = $" (filtered by {string.Join(", ", filters)})";
                }

                return new PagedApiResponse<NotificationDto>(
                    notificationDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize)
                {
                    Message = $"Retrieved {notificationDtos.Count} out of {totalCount} notifications for user{filterInfo}, sorted by {parameters.SortBy} {(parameters.SortAscending ? "ascending" : "descending")}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving paged notifications for user ID: {UserId}", userId);
                return new PagedApiResponse<NotificationDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        // Enhanced detailed notification methods
        public async Task<ApiResponse<NotificationDto>> CreateDetailedAppointmentNotificationAsync(Guid userId, DonationAppointmentRequestDto appointment, string status, string additionalInfo = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                string title = GetAppointmentNotificationTitle(status);
                string message = BuildDetailedAppointmentMessage(appointment, status, additionalInfo);
                string notificationType = GetAppointmentNotificationType(status);

                var notification = new Notification
                {
                    Title = title,
                    Type = notificationType,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Send detailed email
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendDetailedAppointmentEmailAsync(user, appointment, status, additionalInfo);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Detailed appointment notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating detailed appointment notification for user ID: {UserId}", userId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateDetailedDonationEventNotificationAsync(Guid userId, DonationEventDto donationEvent, string status, string additionalInfo = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                string title = GetDonationEventNotificationTitle(status);
                string message = BuildDetailedDonationEventMessage(donationEvent, status, additionalInfo);
                string notificationType = GetDonationEventNotificationType(status);

                var notification = new Notification
                {
                    Title = title,
                    Type = notificationType,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Send detailed email
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendDetailedDonationEventEmailAsync(user, donationEvent, status, additionalInfo);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Detailed donation event notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating detailed donation event notification for user ID: {UserId}", userId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private string GetDonationEventNotificationTitle(string status) =>
            status.ToLower() switch
            {
                "completed" => "Donation Completed",
                "started" => "Donation Started",
                "checkedin" => "Donation Check-in Complete",
                "cancelled" => "Donation Cancelled",
                "healthcheckpassed" => "Health Check Passed",
                "healthcheckfailed" => "Health Check Failed",
                "processing" => "Donation Processing",
                "rescheduled" => "Donation Rescheduled",
                _ => $"Donation {status}"
            };

        private string GetDonationEventNotificationType(string status) =>
            status.ToLower() switch
            {
                "completed" => NotificationTypes.DonationCompleted,
                "started" => NotificationTypes.DonationStarted,
                "checkedin" => NotificationTypes.DonationCheckIn,
                "cancelled" => NotificationTypes.DonationCancelled,
                "healthcheckpassed" => NotificationTypes.HealthCheckPassed,
                "healthcheckfailed" => NotificationTypes.HealthCheckFailed,
                "processing" => NotificationTypes.DonationProcessing,
                "rescheduled" => NotificationTypes.AppointmentModified,
                _ => NotificationTypes.DonationProcessUpdate
            };

        public async Task<ApiResponse<NotificationDto>> CreateDonationProcessNotificationAsync(Guid userId, DonationEventDto donationEvent, string processStep, string details = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                string title = $"Donation Process Update - {processStep}";
                string message = BuildDonationProcessMessage(donationEvent, processStep, details);

                var notification = new Notification
                {
                    Title = title,
                    Type = NotificationTypes.DonationProcessUpdate,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Send email for critical process steps
                if (!string.IsNullOrEmpty(user.Email) && IsCriticalProcessStep(processStep))
                {
                    await SendDonationProcessEmailAsync(user, donationEvent, processStep, details);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Donation process notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donation process notification for user ID: {UserId}", userId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }



        public async Task<ApiResponse<NotificationDto>> CreateDetailedBloodRequestNotificationAsync(Guid userId, Guid requestId, string requestType, string status, string bloodType, string location, string additionalDetails = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                bool isEmergency = requestType.ToLower() == "emergency";
                string title = $"{(isEmergency ? "Emergency " : "")}Blood Request {status}";
                string message = BuildDetailedBloodRequestMessage(requestId, requestType, status, bloodType, location, additionalDetails);
                string notificationType = isEmergency ? GetEmergencyRequestNotificationType(status) : GetBloodRequestNotificationType(status);

                var notification = new Notification
                {
                    Title = title,
                    Type = notificationType,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Send detailed email
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendDetailedBloodRequestEmailAsync(user, requestId, requestType, status, bloodType, location, additionalDetails);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Detailed blood request notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating detailed blood request notification for user ID: {UserId}", userId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateEmergencyNotificationAsync(Guid userId, string emergencyType, string details, string urgencyLevel = "High")
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                string title = $"URGENT: {emergencyType}";
                string message = $"Emergency Alert ({urgencyLevel} Priority): {details}";

                var notification = new Notification
                {
                    Title = title,
                    Type = NotificationTypes.EmergencyRequestUrgent,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Always send email for emergency notifications
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendEmergencyEmailAsync(user, emergencyType, details, urgencyLevel);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Emergency notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emergency notification for user ID: {UserId}", userId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateSystemNotificationAsync(Guid userId, string notificationType, string title, string message, string priority = "Normal")
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                var notification = new Notification
                {
                    Title = $"[{priority.ToUpper()}] {title}",
                    Type = notificationType,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Send email for high priority system notifications
                if (!string.IsNullOrEmpty(user.Email) && priority.ToLower() == "high")
                {
                    await SendSystemNotificationEmailAsync(user, title, message, priority);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "System notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating system notification for user ID: {UserId}", userId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> CreateBulkNotificationsAsync(List<Guid> userIds, string title, string type, string message)
        {
            try
            {
                var notifications = new List<Notification>();
                var notificationDtos = new List<NotificationDto>();

                foreach (var userId in userIds)
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId);
                    if (user != null)
                    {
                        var notification = new Notification
                        {
                            Title = title,
                            Type = type,
                            Message = message,
                            IsRead = false,
                            UserId = userId,
                            CreatedTime = DateTimeOffset.Now
                        };

                        notifications.Add(notification);
                        await _unitOfWork.Notifications.AddAsync(notification);

                        // Send email if important notification type
                        if (!string.IsNullOrEmpty(user.Email) && ShouldSendEmail(type))
                        {
                            await SendBulkNotificationEmailAsync(user, title, message, type);
                        }
                    }
                }

                await _unitOfWork.CompleteAsync();

                // Fetch created notifications with user details
                foreach (var notification in notifications)
                {
                    var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                    notificationDtos.Add(MapToDto(createdNotification));
                }

                return new ApiResponse<List<NotificationDto>>(notificationDtos, $"Created {notificationDtos.Count} bulk notifications successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating bulk notifications");
                return new ApiResponse<List<NotificationDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<List<NotificationDto>>> CreateRoleBasedNotificationAsync(string roleName, string title, string type, string message)
        {
            try
            {
                var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
                if (role == null)
                {
                    return new ApiResponse<List<NotificationDto>>(HttpStatusCode.BadRequest, $"Role '{roleName}' not found");
                }

                var users = await _unitOfWork.Users.FindAsync(u => u.RoleId == role.Id);
                var userIds = users.Select(u => u.Id).ToList();

                return await CreateBulkNotificationsAsync(userIds, title, type, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role-based notifications for role: {RoleName}", roleName);
                return new ApiResponse<List<NotificationDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateDonorRestReminderAsync(Guid donorUserId, DateTimeOffset donationDate)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(donorUserId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {donorUserId} not found");
                }

                // Convert to Vietnam time zone
                var localDonationDate = ConvertToVietnamTimeZone(donationDate);

                var notification = new Notification
                {
                    Title = "Donor Rest Reminder",
                    Type = NotificationTypes.DonationRestReminder,
                    Message = $"Thank you for your blood donation on {localDonationDate:dd/MM/yyyy}. Please remember to rest adequately, " +
                              $"stay hydrated, and avoid strenuous activities for the next 24 hours. " +
                              $"If you experience any discomfort, please consult a healthcare professional.",
                    IsRead = false,
                    UserId = donorUserId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendDonorRestReminderEmailAsync(user, donationDate);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Donor rest reminder created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating donor rest reminder for user ID: {UserId}", donorUserId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateNextDonationDateReminderAsync(Guid donorUserId, DateTimeOffset nextAvailableDate)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(donorUserId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {donorUserId} not found");
                }

                // Convert to Vietnam time zone
                var localNextAvailableDate = ConvertToVietnamTimeZone(nextAvailableDate);

                var notification = new Notification
                {
                    Title = "Next Donation Date",
                    Type = NotificationTypes.NextDonationDate,
                    Message = $"You will be eligible to donate blood again on {localNextAvailableDate:dd/MM/yyyy}. " +
                              $"We will send you a reminder when this date approaches. Thank you for your contribution to saving lives!",
                    IsRead = false,
                    UserId = donorUserId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendNextDonationDateEmailAsync(user, nextAvailableDate);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Next donation date reminder created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating next donation date reminder for user ID: {UserId}", donorUserId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateBloodRequestStatusNotificationAsync(Guid userId, Guid requestId, string status, bool isEmergency = false)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                string title;
                string type;
                string message;

                if (isEmergency)
                {
                    title = $"Emergency Request {status}";
                    type = status == "Fulfilled" ? NotificationTypes.EmergencyRequestFulfilled : NotificationTypes.EmergencyRequest;
                    message = $"Your emergency blood request (ID: {requestId}) has been {status.ToLower()}.";
                }
                else
                {
                    title = $"Blood Request {status}";
                    type = status == "Fulfilled" ? NotificationTypes.BloodRequestFulfilled : NotificationTypes.BloodRequestUpdate;
                    message = $"Your blood request (ID: {requestId}) has been {status.ToLower()}.";
                }

                if (status == "Fulfilled")
                {
                    message += " The blood units are now available for collection.";
                }
                else if (status == "Processing")
                {
                    message += " Our team is working on your request.";
                }
                else if (status == "Cancelled")
                {
                    message += " If you have any questions, please contact our support team.";
                }

                var notification = new Notification
                {
                    Title = title,
                    Type = type,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendBloodRequestStatusEmailAsync(user, requestId, status, isEmergency);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Blood request status notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating blood request status notification. UserId: {UserId}, RequestId: {RequestId}, Status: {Status}",
                    userId, requestId, status);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateAppointmentNotificationAsync(Guid userId, Guid appointmentId, string status, DateTimeOffset date, string locationName)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                // Get full appointment details for a more detailed notification
                var appointment = await _unitOfWork.DonationAppointmentRequests.GetByIdWithDetailsAsync(appointmentId);
                if (appointment == null)
                {
                    _logger.LogWarning("Could not find detailed appointment information for ID: {AppointmentId}", appointmentId);
                }

                string title;
                string type;
                string message;
                string timeSlot = "";
                string locationAddress = "";
                string bloodGroupName = "";
                string componentTypeName = "";

                // The effective date is the confirmed date if available, otherwise use the preferred date
                var effectiveDate = appointment?.ConfirmedDate ?? date;

                // Always convert to Vietnam time zone
                effectiveDate = ConvertToVietnamTimeZone(effectiveDate);

                // Extract time slot from location name if present
                if (locationName.Contains("[") && locationName.Contains("]"))
                {
                    int startIndex = locationName.IndexOf("[") + 1;
                    int endIndex = locationName.IndexOf("]", startIndex);
                    if (endIndex > startIndex)
                    {
                        timeSlot = locationName.Substring(startIndex, endIndex - startIndex);
                        locationName = locationName.Substring(0, startIndex - 1).Trim();
                    }
                }

                // Extract additional details if appointment is available
                if (appointment != null)
                {
                    if (string.IsNullOrEmpty(timeSlot) && !string.IsNullOrEmpty(appointment.PreferredTimeSlot))
                    {
                        timeSlot = appointment.PreferredTimeSlot;
                    }

                    // Use confirmed time slot if available
                    if (!string.IsNullOrEmpty(appointment.ConfirmedTimeSlot))
                    {
                        timeSlot = appointment.ConfirmedTimeSlot;
                    }

                    if (appointment.Location != null)
                    {
                        locationAddress = appointment.Location.Address;
                    }
                    else if (appointment.ConfirmedLocation != null)
                    {
                        locationAddress = appointment.ConfirmedLocation.Address;
                    }

                    if (appointment.BloodGroup != null)
                    {
                        bloodGroupName = appointment.BloodGroup.GroupName;
                    }

                    if (appointment.ComponentType != null)
                    {
                        componentTypeName = appointment.ComponentType.Name;
                    }
                }

                // Format the effective date and time for better display
                string formattedEffectiveDate = effectiveDate.ToString("dddd, dd/MM/yyyy");
                string formattedEffectiveTime = GetTimeSlotWithHours(timeSlot);

                switch (status.ToLower())
                {
                    case "approved":
                        title = $"Appointment Approved for {formattedEffectiveDate}";
                        type = NotificationTypes.AppointmentApproved;
                        message = $"Your blood donation appointment on {formattedEffectiveDate} at {formattedEffectiveTime} ({locationName}) has been approved.";
                        break;
                    case "rejected":
                        title = "Appointment Rejected";
                        type = NotificationTypes.AppointmentRejected;
                        message = $"Your blood donation appointment request for {formattedEffectiveDate} at {locationName} has been rejected.";
                        break;
                    case "cancelled":
                        title = "Appointment Cancelled";
                        type = NotificationTypes.AppointmentCancelled;
                        message = $"Your blood donation appointment on {formattedEffectiveDate} at {locationName} has been cancelled.";
                        break;
                    case "pending":
                        title = "Appointment Request Received";
                        type = NotificationTypes.AppointmentCreated;
                        message = $"Your blood donation appointment request for {formattedEffectiveDate} at {locationName} has been received and is pending approval.";
                        break;
                    case "reminder":
                        title = $"Appointment Reminder: {formattedEffectiveDate}";
                        type = NotificationTypes.AppointmentReminder;
                        message = $"Reminder: You have a blood donation appointment scheduled for {formattedEffectiveDate} at {formattedEffectiveTime} ({locationName}).";
                        break;
                    case "checkedin":
                        title = "Appointment Check-in Complete";
                        type = NotificationTypes.AppointmentCheckIn;
                        message = $"You've been checked in for your blood donation appointment at {locationName} on {formattedEffectiveDate}.";
                        break;
                    default:
                        title = "Appointment Update";
                        type = NotificationTypes.AppointmentUpdate;
                        message = $"Your blood donation appointment (ID: {appointmentId}) has been updated. Status: {status}";
                        break;
                }

                // Build a more detailed message with all available information
                StringBuilder detailedMessage = new StringBuilder();
                detailedMessage.AppendLine($"<h4>Appointment {status}</h4>");
                detailedMessage.AppendLine("<hr/>");

                // Highlight the effective date and time
                detailedMessage.AppendLine("<div class='highlight'>");
                detailedMessage.AppendLine("<h4>📅 Effective Appointment Date and Time:</h4>");
                detailedMessage.AppendLine($"<p><strong>Date:</strong> {formattedEffectiveDate}</p>");
                detailedMessage.AppendLine($"<p><strong>Time:</strong> {formattedEffectiveTime}</p>");
                detailedMessage.AppendLine("</div>");

                detailedMessage.AppendLine("<h4>📋 Appointment Details:</h4>");
                detailedMessage.AppendLine("<ul>");

                // Display status information
                detailedMessage.AppendLine($"<li><strong>Status:</strong> <span style='color: #e74c3c;'>{status}</span></li>");

                detailedMessage.AppendLine($"<li><strong>Location:</strong> {locationName}</li>");

                if (!string.IsNullOrEmpty(locationAddress))
                {
                    detailedMessage.AppendLine($"<li><strong>Address:</strong> {locationAddress}</li>");
                }

                if (!string.IsNullOrEmpty(bloodGroupName))
                {
                    detailedMessage.AppendLine($"<li><strong>Blood Group:</strong> {bloodGroupName}</li>");
                }

                if (!string.IsNullOrEmpty(componentTypeName))
                {
                    detailedMessage.AppendLine($"<li><strong>Donation Type:</strong> {componentTypeName}</li>");
                }

                // Add appointment ID for reference
                detailedMessage.AppendLine($"<li><strong>Appointment ID:</strong> {appointmentId}</li>");

                detailedMessage.AppendLine("</ul>");

                // Add status-specific guidance
                detailedMessage.AppendLine("<hr/>");
                detailedMessage.AppendLine("<h4>✅ Important Information:</h4>");

                switch (status.ToLower())
                {
                    case "approved":
                        detailedMessage.AppendLine("<ul>");
                        detailedMessage.AppendLine("<li>Please bring a valid photo ID</li>");
                        detailedMessage.AppendLine("<li>Eat a healthy meal before donating</li>");
                        detailedMessage.AppendLine("<li>Stay well hydrated before your appointment</li>");
                        detailedMessage.AppendLine("<li>Get adequate rest the night before</li>");
                        detailedMessage.AppendLine("</ul>");
                        break;

                    case "reminder":
                        detailedMessage.AppendLine("<p class='highlight'>Your appointment is coming up soon. Please remember to:</p>");
                        detailedMessage.AppendLine("<ul>");
                        detailedMessage.AppendLine("<li>Bring a valid photo ID</li>");
                        detailedMessage.AppendLine("<li>Get plenty of rest before your appointment</li>");
                        detailedMessage.AppendLine("<li>Eat a nutritious meal 2-3 hours before donating</li>");
                        detailedMessage.AppendLine("<li>Drink extra water before your appointment</li>");
                        detailedMessage.AppendLine("</ul>");
                        break;

                    case "checkedin":
                        detailedMessage.AppendLine("<p>You have been successfully checked in for your blood donation appointment.</p>");
                        detailedMessage.AppendLine("<p>Our staff will guide you through the donation process shortly.</p>");
                        break;
                }

                var notification = new Notification
                {
                    Title = title,
                    Type = type,
                    Message = detailedMessage.ToString(),
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                if (!string.IsNullOrEmpty(user.Email))
                {
                    string donorName = $"{user.FirstName} {user.LastName}";
                    string notes = null;

                    if (status.ToLower() == "rejected")
                    {
                        notes = "Your appointment request could not be accommodated at this time. Please try a different date or location.";
                    }
                    else if (status.ToLower() == "approved")
                    {
                        notes = $"Please remember to bring a valid ID and be well-rested before your donation on {formattedEffectiveDate} at {formattedEffectiveTime}. Avoid heavy meals but stay hydrated.";
                    }

                    await _emailService.SendAppointmentEmailAsync(
                        user.Email,
                        donorName,
                        effectiveDate,
                        !string.IsNullOrEmpty(timeSlot) ? timeSlot : "Not specified",
                        locationName,
                        status,
                        notes);

                    _logger.LogInformation("Appointment {Status} email sent to user ID: {UserId}", status, userId);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Appointment notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment notification. UserId: {UserId}, AppointmentId: {AppointmentId}, Status: {Status}",
                    userId, appointmentId, status);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Placeholder methods for interface compliance
        public async Task<ApiResponse<NotificationDto>> ScheduleNotificationAsync(CreateNotificationDto notificationDto, DateTimeOffset scheduleTime)
        {
            // TODO: Implement scheduled notifications
            throw new NotImplementedException("Scheduled notifications not yet implemented");
        }

        public async Task<ApiResponse> CancelScheduledNotificationAsync(Guid notificationId)
        {
            // TODO: Implement cancel scheduled notifications
            throw new NotImplementedException("Cancel scheduled notifications not yet implemented");
        }

        public async Task<ApiResponse<bool>> CheckUserNotificationPreferencesAsync(Guid userId, string notificationType)
        {
            // TODO: Implement user notification preferences
            return new ApiResponse<bool>(true, "User notification preferences check - default to true");
        }

        public async Task<ApiResponse<NotificationDto>> CreateNotificationWithPreferencesAsync(CreateNotificationDto notificationDto, bool forceEmail = false)
        {
            // For now, just call the regular create method
            return await CreateNotificationAsync(notificationDto);
        }

        #region Helper Methods

        private NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                Type = notification.Type,
                Message = notification.Message,
                IsRead = notification.IsRead,
                UserId = notification.UserId,
                UserName = notification.User != null ? $"{notification.User.FirstName} {notification.User.LastName}" : string.Empty,
                CreatedTime = notification.CreatedTime,
                LastUpdatedTime = notification.LastUpdatedTime
            };
        }

        private bool ShouldSendEmail(string notificationType)
        {
            var emailNotificationTypes = new[]
            {
                NotificationTypes.AppointmentApproved,
                NotificationTypes.AppointmentRejected,
                NotificationTypes.AppointmentCancelled,
                NotificationTypes.AppointmentReminder,
                NotificationTypes.DonationCompleted,
                NotificationTypes.PostDonationCare,
                NotificationTypes.NextDonationDate,
                NotificationTypes.HealthCheckFailed,
                NotificationTypes.BloodRequestFulfilled,
                NotificationTypes.EmergencyRequest,
                NotificationTypes.SystemMaintenance,
                NotificationTypes.SecurityAlert
            };

            return emailNotificationTypes.Contains(notificationType);
        }

        private async Task SendNotificationEmailAsync(User user, Notification notification)
        {
            try
            {
                string subject = $"Blood Donation System - {notification.Title}";
                string body = BuildGenericEmailBody(user, notification);
                await _emailService.SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation("Email sent for notification {NotificationId} to user {UserId}", notification.Id, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email for notification {NotificationId}", notification.Id);
            }
        }

        private string BuildGenericEmailBody(User user, Notification notification)
        {
            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("<!DOCTYPE html>");
            bodyBuilder.AppendLine("<html>");
            bodyBuilder.AppendLine("<head>");
            bodyBuilder.AppendLine("<style>");
            bodyBuilder.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            bodyBuilder.AppendLine(".container { width: 600px; margin: 0 auto; padding: 20px; }");
            bodyBuilder.AppendLine(".header { background-color: #e74c3c; color: white; padding: 10px; text-align: center; }");
            bodyBuilder.AppendLine(".content { padding: 20px; background-color: #f9f9f9; }");
            bodyBuilder.AppendLine(".footer { font-size: 12px; text-align: center; margin-top: 20px; color: #777; }");
            bodyBuilder.AppendLine("</style>");
            bodyBuilder.AppendLine("</head>");
            bodyBuilder.AppendLine("<body>");
            bodyBuilder.AppendLine("<div class='container'>");
            bodyBuilder.AppendLine("<div class='header'>");
            bodyBuilder.AppendLine("<h2>Blood Donation Support System</h2>");
            bodyBuilder.AppendLine("</div>");
            bodyBuilder.AppendLine("<div class='content'>");
            bodyBuilder.AppendLine($"<p>Dear <b>{user.FirstName} {user.LastName}</b>,</p>");
            bodyBuilder.AppendLine($"<h3>{notification.Title}</h3>");
            bodyBuilder.AppendLine($"<p>{notification.Message}</p>");
            bodyBuilder.AppendLine("<p>Best regards,<br/>Blood Donation Support Team</p>");
            bodyBuilder.AppendLine("</div>");
            bodyBuilder.AppendLine("<div class='footer'>");
            bodyBuilder.AppendLine("<p>© 2023 Blood Donation Support System. All rights reserved.</p>");
            bodyBuilder.AppendLine("</div>");
            bodyBuilder.AppendLine("</div>");
            bodyBuilder.AppendLine("</body>");
            bodyBuilder.AppendLine("</html>");

            return bodyBuilder.ToString();
        }

        private string BuildDonationProcessMessage(DonationEventDto donationEvent, string processStep, string details)
        {
            var message = new StringBuilder();
            message.AppendLine($"Donation Process Update: {processStep}");
            message.AppendLine($"Location: {donationEvent.LocationName}");
            if (!string.IsNullOrEmpty(details))
                message.AppendLine($"Details: {details}");

            return message.ToString();
        }

        private bool IsCriticalProcessStep(string processStep) =>
            processStep.ToLower() switch
            {
                "completed" => true,
                "complication" => true,
                "healthcheckfailed" => true,
                _ => false
            };

        private string GetBloodRequestNotificationType(string status) =>
            status.ToLower() switch
            {
                "fulfilled" => NotificationTypes.BloodRequestFulfilled,
                "approved" => NotificationTypes.BloodRequestApproved,
                "cancelled" => NotificationTypes.BloodRequestCancelled,
                "processing" => NotificationTypes.BloodRequestProcessing,
                _ => NotificationTypes.BloodRequestUpdate
            };

        private string GetEmergencyRequestNotificationType(string status) =>
            status.ToLower() switch
            {
                "fulfilled" => NotificationTypes.EmergencyRequestFulfilled,
                "approved" => NotificationTypes.EmergencyRequestApproval,
                "cancelled" => NotificationTypes.EmergencyRequestCancelled,
                "urgent" => NotificationTypes.EmergencyRequestUrgent,
                _ => NotificationTypes.EmergencyRequest
            };

        private string BuildDetailedBloodRequestMessage(Guid requestId, string requestType, string status, string bloodType, string location, string additionalDetails)
        {
            var message = new StringBuilder();
            message.AppendLine($"<h4>Blood Request Status: <span style='color: #e74c3c;'>{status}</span></h4>");
            message.AppendLine("<hr/>");

            // Request details section
            message.AppendLine("<h4>📋 Request Details:</h4>");
            message.AppendLine("<ul>");
            message.AppendLine($"<li><strong>Request ID:</strong> {requestId}</li>");
            message.AppendLine($"<li><strong>Request Type:</strong> {requestType}</li>");
            message.AppendLine($"<li><strong>Blood Type Needed:</strong> {bloodType}</li>");
            message.AppendLine($"<li><strong>Location:</strong> {location}</li>");
            message.AppendLine("</ul>");

            // Status-specific information
            message.AppendLine("<hr/>");
            message.AppendLine("<h4>ℹ️ Status Information:</h4>");

            switch (status.ToLower())
            {
                case "created":
                    message.AppendLine("<p>Your blood request has been successfully created and is now being processed.</p>");
                    message.AppendLine("<p>Our team will review your request and begin matching available donors.</p>");
                    break;

                case "processing":
                    message.AppendLine("<p>Your blood request is currently being processed by our team.</p>");
                    message.AppendLine("<p>We are actively working to fulfill your request as soon as possible.</p>");
                    break;

                case "approved":
                    message.AppendLine("<p>Your blood request has been approved and is now being fulfilled.</p>");
                    message.AppendLine("<p>Our team is coordinating with donors to meet your blood requirements.</p>");
                    break;

                case "fulfilled":
                    message.AppendLine("<div class='success'>");
                    message.AppendLine("<p><strong>Good news!</strong> Your blood request has been fulfilled.</p>");
                    message.AppendLine("<p>The requested blood units are now available for collection.</p>");
                    message.AppendLine("<p>Please contact the blood bank to arrange collection details.</p>");
                    message.AppendLine("</div>");
                    break;

                case "partially fulfilled":
                    message.AppendLine("<div class='warning'>");
                    message.AppendLine("<p>Your blood request has been partially fulfilled.</p>");
                    message.AppendLine("<p>Some of the requested blood units are available, while we continue to work on fulfilling the remainder.</p>");
                    message.AppendLine("<p>Please contact the blood bank for details on what is currently available.</p>");
                    message.AppendLine("</div>");
                    break;

                case "cancelled":
                    message.AppendLine("<div class='info'>");
                    message.AppendLine("<p>Your blood request has been cancelled as requested.</p>");
                    message.AppendLine("<p>If you need to submit a new request, please do so through the system.</p>");
                    message.AppendLine("</div>");
                    break;

                case "expired":
                    message.AppendLine("<div class='warning'>");
                    message.AppendLine("<p>Your blood request has expired.</p>");
                    message.AppendLine("<p>If you still need blood, please submit a new request or contact our support team.</p>");
                    message.AppendLine("</div>");
                    break;

                default:
                    message.AppendLine($"<p>Your blood request status is now: {status}</p>");
                    message.AppendLine("<p>Our team will keep you updated on any changes to your request.</p>");
                    break;
            }

            // Additional details if provided
            if (!string.IsNullOrEmpty(additionalDetails))
            {
                message.AppendLine("<hr/>");
                message.AppendLine("<h4>📝 Additional Information:</h4>");
                message.AppendLine($"<p>{additionalDetails}</p>");
            }

            // Contact information
            message.AppendLine("<hr/>");
            message.AppendLine("<p><i>If you have any questions about your blood request, please contact our support team.</i></p>");

            return message.ToString();
        }

        private string BuildDetailedEmergencyRequestMessage(string emergencyType, string status, string bloodType, string location, string urgencyLevel, string details)
        {
            var message = new StringBuilder();
            message.AppendLine($"<h4><span style='color: #dc3545;'>⚠️ EMERGENCY</span> Blood Request Status: <span style='color: #e74c3c;'>{status}</span></h4>");
            message.AppendLine("<hr/>");

            // Emergency request details section
            message.AppendLine("<h4>🚨 Emergency Request Details:</h4>");
            message.AppendLine("<ul>");
            message.AppendLine($"<li><strong>Emergency Type:</strong> {emergencyType}</li>");
            message.AppendLine($"<li><strong>Blood Type Needed:</strong> {bloodType}</li>");
            message.AppendLine($"<li><strong>Location:</strong> {location}</li>");
            message.AppendLine($"<li><strong>Urgency Level:</strong> <span style='color: #dc3545; font-weight: bold;'>{urgencyLevel}</span></li>");
            message.AppendLine("</ul>");

            // Status-specific information
            message.AppendLine("<hr/>");
            message.AppendLine("<h4>ℹ️ Status Information:</h4>");

            switch (status.ToLower())
            {
                case "created":
                    message.AppendLine("<div class='danger'>");
                    message.AppendLine("<p>Your emergency blood request has been registered with our highest priority.</p>");
                    message.AppendLine("<p>Our emergency response team has been notified and is taking immediate action.</p>");
                    message.AppendLine("</div>");
                    break;

                case "processing":
                    message.AppendLine("<div class='warning'>");
                    message.AppendLine("<p>Your emergency blood request is being processed with the highest priority.</p>");
                    message.AppendLine("<p>Our team is coordinating with nearby donors and blood banks to fulfill your request urgently.</p>");
                    message.AppendLine("</div>");
                    break;

                case "approved":
                    message.AppendLine("<div class='info'>");
                    message.AppendLine("<p>Your emergency blood request has been approved and assigned top priority.</p>");
                    message.AppendLine("<p>Blood units are being prepared and will be available as soon as possible.</p>");
                    message.AppendLine("</div>");
                    break;

                case "fulfilled":
                    message.AppendLine("<div class='success'>");
                    message.AppendLine("<p><strong>Emergency blood request fulfilled!</strong></p>");
                    message.AppendLine("<p>The requested blood units are now available for immediate use.</p>");
                    message.AppendLine("<p>Please contact the blood bank immediately to arrange collection or delivery.</p>");
                    message.AppendLine("</div>");
                    break;

                case "partially fulfilled":
                    message.AppendLine("<div class='warning'>");
                    message.AppendLine("<p>Your emergency blood request has been partially fulfilled.</p>");
                    message.AppendLine("<p>Some blood units are available now, and we continue to work with utmost urgency to secure the remaining units.</p>");
                    message.AppendLine("<p>Please contact the blood bank immediately for details on what is currently available.</p>");
                    message.AppendLine("</div>");
                    break;

                case "escalated":
                    message.AppendLine("<div class='danger'>");
                    message.AppendLine("<p>Your emergency blood request has been escalated to regional blood banks.</p>");
                    message.AppendLine("<p>We are coordinating with multiple facilities to meet your urgent need as quickly as possible.</p>");
                    message.AppendLine("</div>");
                    break;

                case "cancelled":
                    message.AppendLine("<div class='info'>");
                    message.AppendLine("<p>Your emergency blood request has been cancelled as requested.</p>");
                    message.AppendLine("<p>If you need to submit a new emergency request, please do so immediately through the system or contact our emergency hotline.</p>");
                    message.AppendLine("</div>");
                    break;

                default:
                    message.AppendLine($"<p>Your emergency blood request status is now: {status}</p>");
                    message.AppendLine("<p>Our emergency team will keep you updated on any changes to your request.</p>");
                    break;
            }

            // Additional details if provided
            if (!string.IsNullOrEmpty(details))
            {
                message.AppendLine("<hr/>");
                message.AppendLine("<h4>📝 Additional Information:</h4>");
                message.AppendLine($"<p>{details}</p>");
            }

            // Emergency contact information
            message.AppendLine("<hr/>");
            message.AppendLine("<div class='danger'>");
            message.AppendLine("<p><strong>Emergency Contact:</strong> For immediate assistance, please call our emergency hotline at <strong>1-800-BLOOD-NOW</strong></p>");
            message.AppendLine("</div>");

            return message.ToString();
        }

        private string GetAppointmentNotificationTitle(string status) =>
            status.ToLower() switch
            {
                "approved" => "Appointment Approved",
                "rejected" => "Appointment Rejected",
                "cancelled" => "Appointment Cancelled",
                "modified" => "Appointment Modified",
                "reminder" => "Appointment Reminder",
                _ => "Appointment Update"
            };

        private string GetAppointmentNotificationType(string status) =>
            status.ToLower() switch
            {
                "approved" => NotificationTypes.AppointmentApproved,
                "rejected" => NotificationTypes.AppointmentRejected,
                "cancelled" => NotificationTypes.AppointmentCancelled,
                "modified" => NotificationTypes.AppointmentModified,
                "reminder" => NotificationTypes.AppointmentReminder,
                _ => NotificationTypes.AppointmentUpdate
            };

        private DateTimeOffset ConvertToVietnamTimeZone(DateTimeOffset utcTime)
        {
            // Check if already in Vietnam timezone (+07:00)
            string timeZoneInfo = utcTime.ToString("zzz");
            if (timeZoneInfo == "+07:00")
            {
                return utcTime;
            }

            // Convert to Vietnam timezone (UTC+7)
            return utcTime.ToOffset(TimeSpan.FromHours(7));
        }

        private string BuildDetailedAppointmentMessage(DonationAppointmentRequestDto appointment, string status, string additionalInfo)
        {
            var message = new StringBuilder();

            // Get the effective date (confirmed or preferred)
            var effectiveDate = appointment.ConfirmedDate ?? appointment.PreferredDate;

            // Always convert to Vietnam time zone
            effectiveDate = ConvertToVietnamTimeZone(effectiveDate);

            // Format date and time separately and combine them in the desired format
            string formattedDate = effectiveDate.ToString("dd/MM/yyyy");
            string formattedTime = effectiveDate.ToString("H:mm");
            string combinedDateTime = $"{formattedTime} {formattedDate}";

            // Determine time slot (confirmed or preferred)
            string timeSlot = !string.IsNullOrEmpty(appointment.ConfirmedTimeSlot)
                ? appointment.ConfirmedTimeSlot
                : appointment.PreferredTimeSlot;

            // Format time slot with exact hours (for additional information)
            string timeSlotDetails = GetTimeSlotWithHours(timeSlot);

            // Start with a more prominent effective date display
            message.AppendLine("<div style='background-color: #e3f2fd; padding: 15px; border-left: 4px solid #2196f3; margin-bottom: 20px;'>");
            message.AppendLine("<h3 style='color: #2196f3; margin-top: 0; text-align: center;'>THÔNG TIN LỊCH HẸN HIẾN MÁU</h3>");

            // Display the combined date and time in the required format prominently
            message.AppendLine("<div style='font-size: 22px; text-align: center; font-weight: bold; color: #e74c3c; margin: 15px 0;'>");
            message.AppendLine($"{combinedDateTime}");
            message.AppendLine("</div>");

            // Add day of week for context
            message.AppendLine($"<p style='text-align: center;'><strong>Ngày:</strong> {effectiveDate.ToString("dddd, dd/MM/yyyy")}</p>");

            // Add time slot description if applicable
            if (!string.IsNullOrEmpty(timeSlot))
            {
                message.AppendLine($"<p style='text-align: center;'><strong>Khung giờ:</strong> {timeSlotDetails}</p>");
            }

            message.AppendLine("</div>");

            // Status notification with status badge
            message.AppendLine("<div style='margin: 15px 0;'>");
            message.AppendLine($"<p>Trạng thái cuộc hẹn: <span style='display: inline-block; padding: 5px 10px; background-color: #e74c3c; color: white; border-radius: 4px; font-weight: bold;'>{status.ToUpper()}</span></p>");
            message.AppendLine("</div>");

            message.AppendLine("<hr/>");
            message.AppendLine("<h4>📋 Chi tiết cuộc hẹn:</h4>");

            // Location information with more details if available
            message.AppendLine($"<p><strong>Địa điểm:</strong> {appointment.ConfirmedLocationName ?? appointment.LocationName}</p>");
            if (!string.IsNullOrEmpty(appointment.LocationAddress))
            {
                message.AppendLine($"<p><strong>Địa chỉ:</strong> {appointment.LocationAddress}</p>");
            }

            // Blood group and component type information
            if (!string.IsNullOrEmpty(appointment.BloodGroupName))
            {
                message.AppendLine($"<p><strong>Nhóm máu:</strong> {appointment.BloodGroupName}</p>");
            }

            if (!string.IsNullOrEmpty(appointment.ComponentTypeName))
            {
                message.AppendLine($"<p><strong>Loại hiến máu:</strong> {appointment.ComponentTypeName}</p>");
            }

            // Status-specific instructions
            message.AppendLine("<hr/>");
            message.AppendLine("<h4>✅ Hướng dẫn quan trọng:</h4>");

            switch (status.ToLower())
            {
                case "approved":
                    message.AppendLine("<ul>");
                    message.AppendLine("<li>Vui lòng mang theo CMND/CCCD hợp lệ</li>");
                    message.AppendLine("<li>Ăn bữa ăn đầy đủ trước khi hiến máu</li>");
                    message.AppendLine("<li>Uống nhiều nước trước cuộc hẹn</li>");
                    message.AppendLine("<li>Đảm bảo nghỉ ngơi đầy đủ đêm trước</li>");
                    message.AppendLine("<li>Mặc trang phục thoải mái, áo có thể xắn tay dễ dàng</li>");
                    message.AppendLine("</ul>");
                    break;

                case "rejected":
                    message.AppendLine("<p>Rất tiếc, chúng tôi không thể tiếp nhận cuộc hẹn của bạn vào lúc này.</p>");
                    message.AppendLine("<p>Bạn có thể đặt lịch hẹn mới vào thời gian hoặc địa điểm khác.</p>");
                    break;

                case "cancelled":
                    message.AppendLine("<p>Cuộc hẹn của bạn đã được hủy theo yêu cầu.</p>");
                    message.AppendLine("<p>Bạn có thể đặt lịch hẹn mới bất cứ khi nào thuận tiện.</p>");
                    break;

                case "checkedin":
                    message.AppendLine("<p>Bạn đã được check-in thành công cho cuộc hẹn hiến máu.</p>");
                    message.AppendLine("<p>Vui lòng chờ nhân viên hướng dẫn bạn qua quy trình hiến máu.</p>");
                    break;

                case "completed":
                    message.AppendLine("<ul>");
                    message.AppendLine("<li>Uống nhiều nước trong 24-48 giờ tiếp theo</li>");
                    message.AppendLine("<li>Tránh vận động mạnh trong 24 giờ</li>");
                    message.AppendLine("<li>Không nâng vật nặng bằng cánh tay đã dùng để hiến máu</li>");
                    message.AppendLine("<li>Giữ băng dán ít nhất 4 giờ</li>");
                    message.AppendLine("</ul>");
                    break;

                default:
                    break;
            }

            // Additional information if provided
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message.AppendLine("<hr/>");
                message.AppendLine("<h4>📝 Thông tin bổ sung:</h4>");
                message.AppendLine($"<p>{additionalInfo}</p>");
            }

            // Contact information
            message.AppendLine("<hr/>");
            message.AppendLine("<p><i>Nếu bạn cần đổi lịch hoặc có thắc mắc, vui lòng liên hệ đội ngũ hỗ trợ của chúng tôi.</i></p>");
            message.AppendLine($"<p><strong>Mã cuộc hẹn:</strong> {appointment.Id}</p>");

            return message.ToString();
        }

        private string GetTimeSlotWithHours(string timeSlot)
        {
            return timeSlot?.ToLower() switch
            {
                "morning" => "Morning (8:00 AM - 12:00 PM)",
                "afternoon" => "Afternoon (1:00 PM - 5:00 PM)",
                "evening" => "Evening (6:00 PM - 9:00 PM)",
                _ => timeSlot ?? "Not specified"
            };
        }

        private string BuildDetailedDonationEventMessage(DonationEventDto donationEvent, string status, string additionalInfo)
        {
            var message = new StringBuilder();
            message.AppendLine($"<h4>Donation Event Status: <span style='color: #e74c3c;'>{status}</span></h4>");
            message.AppendLine("<hr/>");

            // Event details section
            message.AppendLine("<h4>📝 Event Details:</h4>");
            message.AppendLine("<ul>");
            message.AppendLine($"<li><strong>Donation ID:</strong> {donationEvent.Id}</li>");
            message.AppendLine($"<li><strong>Location:</strong> {donationEvent.LocationName}</li>");
            message.AppendLine($"<li><strong>Blood Group:</strong> {donationEvent.BloodGroupName}</li>");
            message.AppendLine($"<li><strong>Component Type:</strong> {donationEvent.ComponentTypeName}</li>");

            if (donationEvent.AppointmentDate.HasValue)
            {
                var localDate = ConvertToVietnamTimeZone(donationEvent.AppointmentDate.Value);
                message.AppendLine($"<li><strong>Scheduled Date:</strong> {localDate.ToString("dddd, dd/MM/yyyy")}</li>");
            }

            if (!string.IsNullOrEmpty(donationEvent.AppointmentLocation))
            {
                message.AppendLine($"<li><strong>Appointment Location:</strong> {donationEvent.AppointmentLocation}</li>");
            }

            if (donationEvent.CheckInTime.HasValue)
            {
                var localCheckInTime = ConvertToVietnamTimeZone(donationEvent.CheckInTime.Value);
                message.AppendLine($"<li><strong>Check-in Time:</strong> {localCheckInTime.ToString("dd/MM/yyyy HH:mm")}</li>");
            }

            if (donationEvent.DonationDate.HasValue)
            {
                var localDonationDate = ConvertToVietnamTimeZone(donationEvent.DonationDate.Value);
                message.AppendLine($"<li><strong>Donation Date:</strong> {localDonationDate.ToString("dd/MM/yyyy HH:mm")}</li>");
            }

            if (donationEvent.CompletedTime.HasValue)
            {
                var localCompletedTime = ConvertToVietnamTimeZone(donationEvent.CompletedTime.Value);
                message.AppendLine($"<li><strong>Completion Time:</strong> {localCompletedTime.ToString("dd/MM/yyyy HH:mm")}</li>");
            }

            message.AppendLine("</ul>");

            // Health metrics when available
            if (!string.IsNullOrEmpty(donationEvent.BloodPressure) ||
                donationEvent.HemoglobinLevel.HasValue ||
                donationEvent.Temperature.HasValue)
            {
                message.AppendLine("<hr/>");
                message.AppendLine("<h4>🩺 Health Metrics:</h4>");
                message.AppendLine("<ul>");

                if (!string.IsNullOrEmpty(donationEvent.BloodPressure))
                {
                    message.AppendLine($"<li><strong>Blood Pressure:</strong> {donationEvent.BloodPressure}</li>");
                }

                if (donationEvent.HemoglobinLevel.HasValue)
                {
                    message.AppendLine($"<li><strong>Hemoglobin Level:</strong> {donationEvent.HemoglobinLevel.Value} g/dL</li>");
                }

                if (donationEvent.Temperature.HasValue)
                {
                    message.AppendLine($"<li><strong>Temperature:</strong> {donationEvent.Temperature.Value} °C</li>");
                }

                message.AppendLine("</ul>");
            }

            // Status-specific information
            message.AppendLine("<hr/>");
            message.AppendLine("<h4>ℹ️ Status Information:</h4>");

            switch (status.ToLower())
            {
                case "completed":
                    message.AppendLine("<p><strong>Your donation has been completed successfully. Thank you!</strong></p>");
                    message.AppendLine("<h5>Post-Donation Care:</h5>");
                    message.AppendLine("<ul>");
                    message.AppendLine("<li>Rest for at least 15 minutes before leaving the donation center</li>");
                    message.AppendLine("<li>Drink extra fluids for the next 48 hours</li>");
                    message.AppendLine("<li>Avoid strenuous physical activity for the next 24 hours</li>");
                    message.AppendLine("<li>Don't lift heavy objects with the arm used for donation</li>");
                    message.AppendLine("<li>Keep the bandage on for at least 4-6 hours</li>");
                    message.AppendLine("<li>Eat healthy meals and snacks</li>");
                    message.AppendLine("</ul>");
                    break;

                case "checkedin":
                    message.AppendLine("<p>You have been checked in for your donation appointment.</p>");
                    message.AppendLine("<p>Please wait for our staff to guide you through the health screening process.</p>");
                    break;

                case "healthcheckpassed":
                    message.AppendLine("<p>You have passed the health screening and are eligible to donate today.</p>");
                    message.AppendLine("<p>Our staff will guide you to the donation area shortly.</p>");
                    break;

                case "healthcheckfailed":
                    message.AppendLine("<p>We're sorry, but you did not meet the health requirements for donation at this time.</p>");
                    if (!string.IsNullOrEmpty(donationEvent.RejectionReason))
                    {
                        message.AppendLine($"<p><strong>Reason:</strong> {donationEvent.RejectionReason}</p>");
                    }
                    message.AppendLine("<p>Please consult with our medical staff for more information and when you might be eligible again.</p>");
                    break;

                case "started":
                    message.AppendLine("<p>Your donation process has started.</p>");
                    message.AppendLine("<p>Please follow the instructions of our medical staff throughout the process.</p>");
                    break;

                default:
                    message.AppendLine($"<p>Your donation event status is now: {status}</p>");
                    break;
            }

            // Additional information if provided
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                message.AppendLine("<hr/>");
                message.AppendLine("<h4>📝 Additional Information:</h4>");
                message.AppendLine($"<p>{additionalInfo}</p>");
            }

            if (!string.IsNullOrEmpty(donationEvent.Notes))
            {
                message.AppendLine("<hr/>");
                message.AppendLine("<h4>📋 Notes:</h4>");
                message.AppendLine($"<p>{donationEvent.Notes}</p>");
            }

            return message.ToString();
        }

        // Email sending helper methods
        private async Task SendDetailedAppointmentEmailAsync(User user, DonationAppointmentRequestDto appointment, string status, string additionalInfo)
        {
            try
            {
                var message = BuildDetailedAppointmentMessage(appointment, status, additionalInfo);
                string subject = $"Appointment {status} - Blood Donation System";
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Detailed appointment email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send detailed appointment email to user {UserId}", user.Id);
            }
        }

        private async Task SendDetailedDonationEventEmailAsync(User user, DonationEventDto donationEvent, string status, string additionalInfo)
        {
            try
            {
                var message = BuildDetailedDonationEventMessage(donationEvent, status, additionalInfo);
                string subject = $"Donation {status} - Blood Donation System";
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Detailed donation event email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send detailed donation event email to user {UserId}", user.Id);
            }
        }

        private async Task SendDonationProcessEmailAsync(User user, DonationEventDto donationEvent, string processStep, string details)
        {
            try
            {
                var message = BuildDonationProcessMessage(donationEvent, processStep, details);
                string subject = $"Donation Process: {processStep} - Blood Donation System";
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Donation process email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send donation process email to user {UserId}", user.Id);
            }
        }

        private async Task SendDetailedBloodRequestEmailAsync(User user, Guid requestId, string requestType, string status, string bloodType, string location, string additionalDetails)
        {
            try
            {
                var message = BuildDetailedBloodRequestMessage(requestId, requestType, status, bloodType, location, additionalDetails);
                string subject = $"Blood Request {status} - Blood Donation System";

                // Add urgency indicator for specific statuses
                if (status.ToLower() == "fulfilled")
                {
                    subject = $"✅ {subject}";
                }
                else if (status.ToLower() == "processing")
                {
                    subject = $"⏳ {subject}";
                }

                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Detailed blood request email sent to user {UserId} for request {RequestId}", user.Id, requestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send detailed blood request email to user {UserId} for request {RequestId}", user.Id, requestId);
            }
        }

        private async Task SendDetailedEmergencyRequestEmailAsync(User user, string emergencyType, string status, string bloodType, string location, string urgencyLevel, string details)
        {
            try
            {
                var message = BuildDetailedEmergencyRequestMessage(emergencyType, status, bloodType, location, urgencyLevel, details);
                string subject = $"URGENT: Emergency Blood Request {status}";

                // Use high priority for emergency request emails
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Emergency blood request email sent to user {UserId} for {EmergencyType}", user.Id, emergencyType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send emergency blood request email to user {UserId}", user.Id);
            }
        }

        private async Task SendEmergencyEmailAsync(User user, string emergencyType, string details, string urgencyLevel)
        {
            try
            {
                string subject = $"URGENT: {emergencyType} - {urgencyLevel} Priority";
                string message = $"Emergency Alert: {details}";
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Emergency email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send emergency email to user {UserId}", user.Id);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateDetailedEmergencyRequestNotificationAsync(
    Guid userId,
    string emergencyType,
    string status,
    string bloodType,
    string location,
    string urgencyLevel = "High",
    string details = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                string title = $"URGENT: {emergencyType} Blood Request {status}";
                string message = BuildDetailedEmergencyRequestMessage(emergencyType, status, bloodType, location, urgencyLevel, details);
                string notificationType = GetEmergencyRequestNotificationType(status);

                var notification = new Notification
                {
                    Title = title,
                    Type = notificationType,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Always send email for emergency notifications
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendDetailedEmergencyRequestEmailAsync(user, emergencyType, status, bloodType, location, urgencyLevel, details);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Detailed emergency request notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating detailed emergency request notification for user ID: {UserId}", userId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<NotificationDto>> CreateDetailedBloodRequestNotificationWithDetailsAsync(
            Guid userId,
            Guid requestId,
            string requestType,
            string status,
            string bloodType,
            string location,
            string additionalDetails = null)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                bool isEmergency = requestType.ToLower() == "emergency";
                string title = $"{(isEmergency ? "Emergency " : "")}Blood Request {status}";
                string message = BuildDetailedBloodRequestMessage(requestId, requestType, status, bloodType, location, additionalDetails);
                string notificationType = isEmergency ? GetEmergencyRequestNotificationType(status) : GetBloodRequestNotificationType(status);

                var notification = new Notification
                {
                    Title = title,
                    Type = notificationType,
                    Message = message,
                    IsRead = false,
                    UserId = userId,
                    CreatedTime = DateTimeOffset.Now
                };

                await _unitOfWork.Notifications.AddAsync(notification);
                await _unitOfWork.CompleteAsync();

                // Send detailed email
                if (!string.IsNullOrEmpty(user.Email))
                {
                    await SendDetailedBloodRequestEmailAsync(user, requestId, requestType, status, bloodType, location, additionalDetails);
                }

                var createdNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(notification.Id);
                return new ApiResponse<NotificationDto>(MapToDto(createdNotification), "Detailed blood request notification created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating detailed blood request notification for user ID: {UserId}", userId);
                return new ApiResponse<NotificationDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task SendSystemNotificationEmailAsync(User user, string title, string message, string priority)
        {
            try
            {
                string subject = $"[{priority.ToUpper()}] System Notification: {title}";
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("System notification email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send system notification email to user {UserId}", user.Id);
            }
        }

        private async Task SendBulkNotificationEmailAsync(User user, string title, string message, string type)
        {
            try
            {
                await _emailService.SendEmailAsync(user.Email, title, BuildHtmlEmailBody(user, title, message));
                _logger.LogInformation("Bulk notification email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bulk notification email to user {UserId}", user.Id);
            }
        }

        private async Task SendDonorRestReminderEmailAsync(User user, DateTimeOffset donationDate)
        {
            try
            {
                var localDonationDate = ConvertToVietnamTimeZone(donationDate);
                string subject = "Important: Post-Donation Care Reminder";
                string message = $"Thank you for your blood donation on {localDonationDate:dd/MM/yyyy}. Please rest and stay hydrated for the next 24 hours.";
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Donor rest reminder email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send donor rest reminder email to user {UserId}", user.Id);
            }
        }

        private async Task SendNextDonationDateEmailAsync(User user, DateTimeOffset nextAvailableDate)
        {
            try
            {
                var localNextAvailableDate = ConvertToVietnamTimeZone(nextAvailableDate);
                string subject = "Your Next Blood Donation Date";
                string message = $"You will be eligible to donate blood again on {localNextAvailableDate:dd/MM/yyyy}. We look forward to seeing you again!";
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Next donation date email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send next donation date email to user {UserId}", user.Id);
            }
        }

        private async Task SendBloodRequestStatusEmailAsync(User user, Guid requestId, string status, bool isEmergency)
        {
            try
            {
                string subject = $"{(isEmergency ? "Emergency " : "")}Blood Request {status}";
                string message = $"Your {(isEmergency ? "emergency " : "")}blood request (ID: {requestId}) has been {status.ToLower()}.";
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Blood request status email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send blood request status email to user {UserId}", user.Id);
            }
        }

        private string BuildHtmlEmailBody(User user, string subject, string message)
{
    var bodyBuilder = new StringBuilder();
    bodyBuilder.AppendLine("<!DOCTYPE html>");
    bodyBuilder.AppendLine("<html lang=\"en\">");
    bodyBuilder.AppendLine("<head>");
    bodyBuilder.AppendLine("<meta charset=\"UTF-8\">");
    bodyBuilder.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
    bodyBuilder.AppendLine("<title>" + subject + "</title>");
    bodyBuilder.AppendLine("<style>");
    // Base styles
    bodyBuilder.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f5f5f5; }");
    bodyBuilder.AppendLine(".container { max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 3px 6px rgba(0,0,0,0.1); }");
    bodyBuilder.AppendLine(".header { background-color: #e74c3c; color: white; padding: 20px; text-align: center; }");
    bodyBuilder.AppendLine(".header h2 { margin: 0; font-size: 24px; }");
    bodyBuilder.AppendLine(".content { padding: 25px; background-color: #ffffff; }");
    bodyBuilder.AppendLine(".greeting { font-size: 18px; margin-bottom: 20px; }");
    bodyBuilder.AppendLine(".message-container { background-color: #f9f9f9; padding: 20px; border-radius: 5px; margin-bottom: 20px; }");
    bodyBuilder.AppendLine(".footer { background-color: #f5f5f5; padding: 15px; text-align: center; font-size: 12px; color: #777; }");
    
    // Component styles
    bodyBuilder.AppendLine("h4 { color: #e74c3c; margin-top: 20px; margin-bottom: 10px; padding-bottom: 5px; border-bottom: 1px solid #eee; }");
    bodyBuilder.AppendLine("hr { border: none; border-top: 1px solid #eee; margin: 20px 0; }");
    bodyBuilder.AppendLine("ul { padding-left: 20px; }");
    bodyBuilder.AppendLine("li { margin-bottom: 8px; }");
    bodyBuilder.AppendLine("p { margin: 10px 0; }");
    bodyBuilder.AppendLine(".button { display: inline-block; background-color: #e74c3c; color: white; text-decoration: none; padding: 10px 20px; border-radius: 5px; margin-top: 15px; }");
    bodyBuilder.AppendLine("a.button { color: white; text-decoration: none; }");
    
    // Utility classes
    bodyBuilder.AppendLine(".highlight { background-color: #fff3cd; padding: 10px; border-left: 4px solid #ffc107; margin: 15px 0; }");
    bodyBuilder.AppendLine(".info { background-color: #e3f2fd; padding: 10px; border-left: 4px solid #2196f3; margin: 15px 0; }");
    bodyBuilder.AppendLine(".success { background-color: #d4edda; padding: 10px; border-left: 4px solid #28a745; margin: 15px 0; }");
    bodyBuilder.AppendLine(".warning { background-color: #fff3cd; padding: 10px; border-left: 4px solid #ffc107; margin: 15px 0; }");
    bodyBuilder.AppendLine(".danger { background-color: #f8d7da; padding: 10px; border-left: 4px solid #dc3545; margin: 15px 0; }");
    
    // Responsive design
    bodyBuilder.AppendLine("@media only screen and (max-width: 620px) { .container { width: 100%; border-radius: 0; } }");
    bodyBuilder.AppendLine("@media only screen and (max-width: 480px) { .content { padding: 15px; } }");
    bodyBuilder.AppendLine("</style>");
    bodyBuilder.AppendLine("</head>");
    bodyBuilder.AppendLine("<body>");
    bodyBuilder.AppendLine("<div class='container'>");
    bodyBuilder.AppendLine("<div class='header'>");
    bodyBuilder.AppendLine("<h2>Blood Donation Support System</h2>");
    bodyBuilder.AppendLine("</div>");
    bodyBuilder.AppendLine("<div class='content'>");
    bodyBuilder.AppendLine($"<p class='greeting'>Dear <b>{user.FirstName} {user.LastName}</b>,</p>");
    bodyBuilder.AppendLine("<div class='message-container'>");
    bodyBuilder.AppendLine($"{message.Replace("\n", "<br/>")}");
    bodyBuilder.AppendLine("</div>");
    bodyBuilder.AppendLine("<p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>");
    bodyBuilder.AppendLine("<p>Best regards,<br/>Blood Donation Support Team</p>");
    bodyBuilder.AppendLine("</div>");
    bodyBuilder.AppendLine("<div class='footer'>");
    bodyBuilder.AppendLine("<p>© " + DateTime.Now.Year + " Blood Donation Support System. All rights reserved.</p>");
    bodyBuilder.AppendLine("<p>This is an automated email. Please do not reply.</p>");
    bodyBuilder.AppendLine("</div>");
    bodyBuilder.AppendLine("</div>");
    bodyBuilder.AppendLine("</body>");
    bodyBuilder.AppendLine("</html>");

    return bodyBuilder.ToString();
}
        #endregion
    }
}