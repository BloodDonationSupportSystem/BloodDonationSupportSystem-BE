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
                // Verify that the user exists
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
                // Verify that the user exists
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
                // Verify that the user exists
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
                // Verify that the user exists
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

                // Fetch the notification with user details
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

                // Fetch the updated notification with user details
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

                // N?u thông báo ?ã ???c ??c, không c?n c?p nh?t
                if (notification.IsRead)
                {
                    var alreadyReadNotification = await _unitOfWork.Notifications.GetByIdWithUserAsync(id);
                    return new ApiResponse<NotificationDto>(MapToDto(alreadyReadNotification), "Notification was already marked as read");
                }

                // ?ánh d?u thông báo là ?ã ??c
                notification.IsRead = true;
                notification.LastUpdatedTime = DateTimeOffset.Now;

                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.CompleteAsync();

                // Fetch the updated notification with user details
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

                // Soft delete - update DeletedTime
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
                // Verify that the user exists
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
                // Verify that the user exists
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

                // Initialize parameters if null
                parameters ??= new NotificationParameters();

                // Set default sorting to CreatedTime descending if not specified
                // This ensures notifications are shown newest first by default
                if (string.IsNullOrEmpty(parameters.SortBy))
                {
                    parameters.SortBy = "createdtime";
                    parameters.SortAscending = false; // Newest first
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

        /// <summary>
        /// Creates a notification for a donor to remind them to rest after donating blood,
        /// and sends a corresponding email if the donor has an email address
        /// </summary>
        public async Task<ApiResponse<NotificationDto>> CreateDonorRestReminderAsync(Guid donorUserId, DateTimeOffset donationDate)
        {
            try
            {
                // Verify that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(donorUserId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {donorUserId} not found");
                }

                // Create notification
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

                // Send email if user has an email address
                if (!string.IsNullOrEmpty(user.Email))
                {
                    string donorName = $"{user.FirstName} {user.LastName}";
                    
                    string emailSubject = "Important: Post-Donation Care Reminder";
                    StringBuilder emailBody = new StringBuilder();
                    emailBody.AppendLine("<!DOCTYPE html>");
                    emailBody.AppendLine("<html>");
                    emailBody.AppendLine("<head>");
                    emailBody.AppendLine("<style>");
                    emailBody.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
                    emailBody.AppendLine(".container { width: 600px; margin: 0 auto; padding: 20px; }");
                    emailBody.AppendLine(".header { background-color: #e74c3c; color: white; padding: 10px; text-align: center; }");
                    emailBody.AppendLine(".content { padding: 20px; background-color: #f9f9f9; }");
                    emailBody.AppendLine(".footer { font-size: 12px; text-align: center; margin-top: 20px; color: #777; }");
                    emailBody.AppendLine("h2 { color: #e74c3c; }");
                    emailBody.AppendLine("ul { margin-left: 20px; }");
                    emailBody.AppendLine("</style>");
                    emailBody.AppendLine("</head>");
                    emailBody.AppendLine("<body>");
                    emailBody.AppendLine("<div class='container'>");
                    emailBody.AppendLine("<div class='header'>");
                    emailBody.AppendLine("<h2>Blood Donation Support System</h2>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("<div class='content'>");
                    emailBody.AppendLine($"<p>Dear <b>{donorName}</b>,</p>");
                    emailBody.AppendLine("<p>Thank you for your generous blood donation today. Your contribution will help save lives!</p>");
                    emailBody.AppendLine("<h3>Important Post-Donation Care:</h3>");
                    emailBody.AppendLine("<ul>");
                    emailBody.AppendLine("<li><strong>Rest:</strong> Take it easy for the next 24 hours. Avoid strenuous activities.</li>");
                    emailBody.AppendLine("<li><strong>Hydrate:</strong> Drink plenty of fluids (non-alcoholic) to rehydrate.</li>");
                    emailBody.AppendLine("<li><strong>Eat Well:</strong> Have nutritious meals to help your body recover.</li>");
                    emailBody.AppendLine("<li><strong>No Heavy Lifting:</strong> Avoid lifting heavy objects for 24 hours.</li>");
                    emailBody.AppendLine("<li><strong>Bandage:</strong> Keep the bandage on for at least 4 hours.</li>");
                    emailBody.AppendLine("</ul>");
                    emailBody.AppendLine("<p><strong>If you experience unusual symptoms</strong> like dizziness, prolonged bleeding, or severe pain, please seek medical attention.</p>");
                    emailBody.AppendLine("<p>Thank you again for your life-saving donation!</p>");
                    emailBody.AppendLine("<p>Best regards,<br/>Blood Donation Support Team</p>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("<div class='footer'>");
                    emailBody.AppendLine("<p>© 2023 Blood Donation Support System. All rights reserved.</p>");
                    emailBody.AppendLine("<p>This is an automated email. Please do not reply.</p>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("</body>");
                    emailBody.AppendLine("</html>");

                    await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody.ToString());
                    _logger.LogInformation("Post-donation care email sent to donor ID: {DonorId}", donorUserId);
                }

                // Fetch the notification with user details
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

        /// <summary>
        /// Creates a notification for a donor about their next eligible donation date,
        /// and sends a corresponding email if the donor has an email address
        /// </summary>
        public async Task<ApiResponse<NotificationDto>> CreateNextDonationDateReminderAsync(Guid donorUserId, DateTimeOffset nextAvailableDate)
        {
            try
            {
                // Verify that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(donorUserId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {donorUserId} not found");
                }

                // Create notification
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

                // Send email if user has an email address
                if (!string.IsNullOrEmpty(user.Email))
                {
                    string donorName = $"{user.FirstName} {user.LastName}";
                    
                    string emailSubject = "Your Next Blood Donation Date";
                    StringBuilder emailBody = new StringBuilder();
                    emailBody.AppendLine("<!DOCTYPE html>");
                    emailBody.AppendLine("<html>");
                    emailBody.AppendLine("<head>");
                    emailBody.AppendLine("<style>");
                    emailBody.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
                    emailBody.AppendLine(".container { width: 600px; margin: 0 auto; padding: 20px; }");
                    emailBody.AppendLine(".header { background-color: #e74c3c; color: white; padding: 10px; text-align: center; }");
                    emailBody.AppendLine(".content { padding: 20px; background-color: #f9f9f9; }");
                    emailBody.AppendLine(".date-box { background-color: #e74c3c; color: white; padding: 10px; text-align: center; font-size: 18px; margin: 20px 0; }");
                    emailBody.AppendLine(".footer { font-size: 12px; text-align: center; margin-top: 20px; color: #777; }");
                    emailBody.AppendLine("h2 { color: #e74c3c; }");
                    emailBody.AppendLine("</style>");
                    emailBody.AppendLine("</head>");
                    emailBody.AppendLine("<body>");
                    emailBody.AppendLine("<div class='container'>");
                    emailBody.AppendLine("<div class='header'>");
                    emailBody.AppendLine("<h2>Blood Donation Support System</h2>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("<div class='content'>");
                    emailBody.AppendLine($"<p>Dear <b>{donorName}</b>,</p>");
                    emailBody.AppendLine("<p>Thank you for your recent blood donation. Your generosity helps save lives!</p>");
                    emailBody.AppendLine("<p>Based on health guidelines, you will be eligible to donate blood again on:</p>");
                    emailBody.AppendLine($"<div class='date-box'>{nextAvailableDate:dddd, MMMM dd, yyyy}</div>");
                    emailBody.AppendLine("<p>We will send you a reminder as this date approaches. If you'd like to schedule your next donation in advance, you can do so through our system once you're eligible.</p>");
                    emailBody.AppendLine("<p>Remember that regular donations are crucial for maintaining adequate blood supplies. Your continued support is deeply appreciated.</p>");
                    emailBody.AppendLine("<p>Best regards,<br/>Blood Donation Support Team</p>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("<div class='footer'>");
                    emailBody.AppendLine("<p>© 2023 Blood Donation Support System. All rights reserved.</p>");
                    emailBody.AppendLine("<p>This is an automated email. Please do not reply.</p>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("</body>");
                    emailBody.AppendLine("</html>");

                    await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody.ToString());
                    _logger.LogInformation("Next donation date email sent to donor ID: {DonorId}", donorUserId);
                }

                // Fetch the notification with user details
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

        /// <summary>
        /// Creates a notification for blood request status changes,
        /// and sends a corresponding email if the user has an email address
        /// </summary>
        public async Task<ApiResponse<NotificationDto>> CreateBloodRequestStatusNotificationAsync(
            Guid userId, Guid requestId, string status, bool isEmergency = false)
        {
            try
            {
                // Verify that the user exists
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

                // Create notification
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

                // Send email if user has an email address
                if (!string.IsNullOrEmpty(user.Email))
                {
                    string userName = $"{user.FirstName} {user.LastName}";
                    string emergencyTag = isEmergency ? "Emergency " : "";
                    
                    string emailSubject = $"{emergencyTag}Blood Request Status Update - {status}";
                    StringBuilder emailBody = new StringBuilder();
                    emailBody.AppendLine("<!DOCTYPE html>");
                    emailBody.AppendLine("<html>");
                    emailBody.AppendLine("<head>");
                    emailBody.AppendLine("<style>");
                    emailBody.AppendLine("body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
                    emailBody.AppendLine(".container { width: 600px; margin: 0 auto; padding: 20px; }");
                    emailBody.AppendLine(".header { background-color: #e74c3c; color: white; padding: 10px; text-align: center; }");
                    emailBody.AppendLine(".content { padding: 20px; background-color: #f9f9f9; }");
                    emailBody.AppendLine(".status-box { background-color: #f9f9f9; border-left: 4px solid #e74c3c; padding: 10px; margin: 20px 0; }");
                    emailBody.AppendLine(".footer { font-size: 12px; text-align: center; margin-top: 20px; color: #777; }");
                    emailBody.AppendLine("h2 { color: #e74c3c; }");
                    emailBody.AppendLine(".highlight { color: #e74c3c; font-weight: bold; }");
                    emailBody.AppendLine("</style>");
                    emailBody.AppendLine("</head>");
                    emailBody.AppendLine("<body>");
                    emailBody.AppendLine("<div class='container'>");
                    emailBody.AppendLine("<div class='header'>");
                    emailBody.AppendLine("<h2>Blood Donation Support System</h2>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("<div class='content'>");
                    emailBody.AppendLine($"<p>Dear <b>{userName}</b>,</p>");
                    
                    emailBody.AppendLine("<div class='status-box'>");
                    emailBody.AppendLine($"<p>Your {(isEmergency ? "emergency " : "")}blood request (ID: <span class='highlight'>{requestId}</span>) status has been updated to: <span class='highlight'>{status}</span>.</p>");
                    emailBody.AppendLine("</div>");
                    
                    if (status == "Fulfilled")
                    {
                        emailBody.AppendLine("<p>Good news! Your blood request has been fulfilled. The blood units are now available for collection.</p>");
                        emailBody.AppendLine("<p>Please contact our blood bank center to arrange the collection or delivery of your blood units.</p>");
                    }
                    else if (status == "Processing")
                    {
                        emailBody.AppendLine("<p>Your blood request is currently being processed by our team. We are working to fulfill your request as quickly as possible.</p>");
                        emailBody.AppendLine("<p>You will receive another notification once your request has been fulfilled or if we need additional information.</p>");
                    }
                    else if (status == "Cancelled")
                    {
                        emailBody.AppendLine("<p>Your blood request has been cancelled. If you did not request this cancellation or have any questions, please contact our support team.</p>");
                    }
                    
                    emailBody.AppendLine("<p>If you have any questions or need further information, please don't hesitate to contact us.</p>");
                    emailBody.AppendLine("<p>Best regards,<br/>Blood Donation Support Team</p>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("<div class='footer'>");
                    emailBody.AppendLine("<p>© 2023 Blood Donation Support System. All rights reserved.</p>");
                    emailBody.AppendLine("<p>This is an automated email. Please do not reply.</p>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("</div>");
                    emailBody.AppendLine("</body>");
                    emailBody.AppendLine("</html>");

                    await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody.ToString());
                    _logger.LogInformation("Blood request status email sent to user ID: {UserId}", userId);
                }

                // Fetch the notification with user details
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

        /// <summary>
        /// Creates an appointment notification,
        /// and sends a corresponding email if the user has an email address
        /// </summary>
        public async Task<ApiResponse<NotificationDto>> CreateAppointmentNotificationAsync(
            Guid userId, Guid appointmentId, string status, DateTimeOffset date, string locationName)
        {
            try
            {
                // Verify that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<NotificationDto>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                string title;
                string type;
                string message;
                string timeSlot = "";

                // Extract time slot if embedded in the locationName
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

                // Create notification
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

                // Send email via the appointment email service
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
                    
                    // Use the specific appointment email method
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

                // Fetch the notification with user details
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
    }
}