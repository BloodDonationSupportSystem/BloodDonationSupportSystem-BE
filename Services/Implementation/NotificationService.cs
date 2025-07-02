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

                var notification = new Notification
                {
                    Title = "Donor Rest Reminder",
                    Type = NotificationTypes.DonationRestReminder,
                    Message = $"Thank you for your blood donation on {donationDate:dd/MM/yyyy}. Please remember to rest adequately, " +
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

                var notification = new Notification
                {
                    Title = "Next Donation Date",
                    Type = NotificationTypes.NextDonationDate,
                    Message = $"You will be eligible to donate blood again on {nextAvailableDate:dd/MM/yyyy}. " +
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

                string title;
                string type;
                string message;
                string timeSlot = "";

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

                switch (status.ToLower())
                {
                    case "approved":
                        title = "Appointment Approved";
                        type = NotificationTypes.AppointmentApproved;
                        message = $"Your blood donation appointment on {date:dd/MM/yyyy} at {locationName} has been approved.";
                        break;
                    case "rejected":
                        title = "Appointment Rejected";
                        type = NotificationTypes.AppointmentRejected;
                        message = $"Your blood donation appointment request for {date:dd/MM/yyyy} at {locationName} has been rejected.";
                        break;
                    case "cancelled":
                        title = "Appointment Cancelled";
                        type = NotificationTypes.AppointmentCancelled;
                        message = $"Your blood donation appointment on {date:dd/MM/yyyy} at {locationName} has been cancelled.";
                        break;
                    case "pending":
                        title = "Appointment Request Received";
                        type = NotificationTypes.AppointmentCreated;
                        message = $"Your blood donation appointment request for {date:dd/MM/yyyy} at {locationName} has been received and is pending approval.";
                        break;
                    case "reminder":
                        title = "Appointment Reminder";
                        type = NotificationTypes.AppointmentReminder;
                        message = $"Reminder: You have a blood donation appointment scheduled for {date:dd/MM/yyyy} at {locationName}.";
                        break;
                    default:
                        title = "Appointment Update";
                        type = NotificationTypes.AppointmentUpdate;
                        message = $"Your blood donation appointment (ID: {appointmentId}) has been updated. Status: {status}";
                        break;
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
                    string donorName = $"{user.FirstName} {user.LastName}";
                    string notes = null;
                    
                    if (status.ToLower() == "rejected")
                    {
                        notes = "Your appointment request could not be accommodated at this time. Please try a different date or location.";
                    }
                    else if (status.ToLower() == "approved")
                    {
                        notes = "Please remember to bring a valid ID and be well-rested before your donation. Avoid heavy meals but stay hydrated.";
                    }
                    
                    await _emailService.SendAppointmentEmailAsync(
                        user.Email,
                        donorName,
                        date,
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
            message.AppendLine($"Blood Request Status Update: {status}");
            message.AppendLine($"Request Details:");
            message.AppendLine($"• Request ID: {requestId}");
            message.AppendLine($"• Type: {requestType}");
            message.AppendLine($"• Blood Type: {bloodType}");
            message.AppendLine($"• Location: {location}");
            if (!string.IsNullOrEmpty(additionalDetails))
                message.AppendLine($"• Additional Details: {additionalDetails}");
            
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

        private string BuildDetailedAppointmentMessage(DonationAppointmentRequestDto appointment, string status, string additionalInfo)
        {
            var message = new StringBuilder();
            message.AppendLine($"Your blood donation appointment has been {status.ToLower()}.");
            message.AppendLine($"Appointment Details:");
            message.AppendLine($"• Date: {appointment.ConfirmedDate?.ToString("dd/MM/yyyy HH:mm") ?? appointment.PreferredDate.ToString("dd/MM/yyyy")}");
            message.AppendLine($"• Location: {appointment.ConfirmedLocationName ?? appointment.LocationName}");
            if (!string.IsNullOrEmpty(appointment.BloodGroupName))
                message.AppendLine($"• Blood Group: {appointment.BloodGroupName}");
            if (!string.IsNullOrEmpty(appointment.ComponentTypeName))
                message.AppendLine($"• Component Type: {appointment.ComponentTypeName}");
            if (!string.IsNullOrEmpty(additionalInfo))
                message.AppendLine($"• Additional Information: {additionalInfo}");
            
            return message.ToString();
        }

        private string GetDonationEventNotificationTitle(string status) =>
            status.ToLower() switch
            {
                "completed" => "Donation Completed",
                "started" => "Donation Started",
                "checkedin" => "Check-in Successful",
                "healthcheckpassed" => "Health Check Passed",
                "healthcheckfailed" => "Health Check Failed",
                _ => "Donation Update"
            };

        private string GetDonationEventNotificationType(string status) =>
            status.ToLower() switch
            {
                "completed" => NotificationTypes.DonationCompleted,
                "started" => NotificationTypes.DonationStarted,
                "checkedin" => NotificationTypes.AppointmentCheckIn,
                "healthcheckpassed" => NotificationTypes.HealthCheckPassed,
                "healthcheckfailed" => NotificationTypes.HealthCheckFailed,
                _ => NotificationTypes.DonationEventUpdate
            };

        private string BuildDetailedDonationEventMessage(DonationEventDto donationEvent, string status, string additionalInfo)
        {
            var message = new StringBuilder();
            message.AppendLine($"Donation Event Status: {status}");
            message.AppendLine($"Event Details:");
            message.AppendLine($"• Location: {donationEvent.LocationName}");
            message.AppendLine($"• Blood Group: {donationEvent.BloodGroupName}");
            message.AppendLine($"• Component Type: {donationEvent.ComponentTypeName}");
            if (donationEvent.DonationDate.HasValue)
                message.AppendLine($"• Donation Date: {donationEvent.DonationDate.Value:dd/MM/yyyy HH:mm}");
            if (donationEvent.QuantityDonated.HasValue)
                message.AppendLine($"• Quantity Donated: {donationEvent.QuantityDonated.Value} ml");
            if (!string.IsNullOrEmpty(additionalInfo))
                message.AppendLine($"• Additional Information: {additionalInfo}");
            
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
                await _emailService.SendEmailAsync(user.Email, subject, BuildHtmlEmailBody(user, subject, message));
                _logger.LogInformation("Detailed blood request email sent to user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send detailed blood request email to user {UserId}", user.Id);
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
                string subject = "Important: Post-Donation Care Reminder";
                string message = $"Thank you for your blood donation on {donationDate:dd/MM/yyyy}. Please rest and stay hydrated for the next 24 hours.";
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
                string subject = "Your Next Blood Donation Date";
                string message = $"You will be eligible to donate blood again on {nextAvailableDate:dd/MM/yyyy}. We look forward to seeing you again!";
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
            bodyBuilder.AppendLine("<html>");
            bodyBuilder.AppendLine("<head>");
            bodyBuilder.AppendLine("<style>");
            bodyBuilder.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; }");
            bodyBuilder.AppendLine(".container { width: 600px; margin: 0 auto; padding: 20px; }");
            bodyBuilder.AppendLine(".header { background-color: #e74c3c; color: white; padding: 15px; text-align: center; border-radius: 5px 5px 0 0; }");
            bodyBuilder.AppendLine(".content { padding: 20px; background-color: #f9f9f9; border: 1px solid #ddd; border-top: none; }");
            bodyBuilder.AppendLine(".footer { font-size: 12px; text-align: center; margin-top: 20px; color: #777; padding: 10px; }");
            bodyBuilder.AppendLine(".highlight { background-color: #fff3cd; padding: 10px; border-left: 4px solid #ffc107; margin: 10px 0; }");
            bodyBuilder.AppendLine("h2 { margin: 0; }");
            bodyBuilder.AppendLine("</style>");
            bodyBuilder.AppendLine("</head>");
            bodyBuilder.AppendLine("<body>");
            bodyBuilder.AppendLine("<div class='container'>");
            bodyBuilder.AppendLine("<div class='header'>");
            bodyBuilder.AppendLine("<h2>Blood Donation Support System</h2>");
            bodyBuilder.AppendLine("</div>");
            bodyBuilder.AppendLine("<div class='content'>");
            bodyBuilder.AppendLine($"<p>Dear <b>{user.FirstName} {user.LastName}</b>,</p>");
            bodyBuilder.AppendLine("<div class='highlight'>");
            bodyBuilder.AppendLine($"<p>{message.Replace("\n", "<br/>")}</p>");
            bodyBuilder.AppendLine("</div>");
            bodyBuilder.AppendLine("<p>If you have any questions or need assistance, please don't hesitate to contact our support team.</p>");
            bodyBuilder.AppendLine("<p>Best regards,<br/>Blood Donation Support Team</p>");
            bodyBuilder.AppendLine("</div>");
            bodyBuilder.AppendLine("<div class='footer'>");
            bodyBuilder.AppendLine("<p>© 2023 Blood Donation Support System. All rights reserved.</p>");
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