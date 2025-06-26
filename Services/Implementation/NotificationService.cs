using BusinessObjects.Dtos;
using BusinessObjects.Models;
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
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                return new PagedApiResponse<NotificationDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
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