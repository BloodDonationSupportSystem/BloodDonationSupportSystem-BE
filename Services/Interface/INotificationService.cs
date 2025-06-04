using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface INotificationService
    {
        Task<ApiResponse<IEnumerable<NotificationDto>>> GetAllNotificationsAsync();
        Task<ApiResponse<NotificationDto>> GetNotificationByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<NotificationDto>>> GetNotificationsByUserIdAsync(Guid userId);
        Task<ApiResponse<IEnumerable<NotificationDto>>> GetUnreadNotificationsByUserIdAsync(Guid userId);
        Task<ApiResponse<int>> GetUnreadCountByUserIdAsync(Guid userId);
        Task<ApiResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto notificationDto);
        Task<ApiResponse<NotificationDto>> UpdateNotificationAsync(Guid id, UpdateNotificationDto notificationDto);
        Task<ApiResponse> DeleteNotificationAsync(Guid id);
        Task<ApiResponse> MarkAllAsReadForUserAsync(Guid userId);
        Task<PagedApiResponse<NotificationDto>> GetPagedNotificationsAsync(NotificationParameters parameters);
        Task<PagedApiResponse<NotificationDto>> GetPagedNotificationsByUserIdAsync(Guid userId, NotificationParameters parameters);
    }
}