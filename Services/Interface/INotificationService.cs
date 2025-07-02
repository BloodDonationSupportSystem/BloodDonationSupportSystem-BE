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
        Task<ApiResponse<NotificationDto>> MarkNotificationAsReadAsync(Guid id);
        Task<ApiResponse> DeleteNotificationAsync(Guid id);
        Task<ApiResponse> MarkAllAsReadForUserAsync(Guid userId);
        Task<PagedApiResponse<NotificationDto>> GetPagedNotificationsAsync(NotificationParameters parameters);
        Task<PagedApiResponse<NotificationDto>> GetPagedNotificationsByUserIdAsync(Guid userId, NotificationParameters parameters);
        
        // New methods for specific notification scenarios
        Task<ApiResponse<NotificationDto>> CreateDonorRestReminderAsync(Guid donorUserId, DateTimeOffset donationDate);
        Task<ApiResponse<NotificationDto>> CreateNextDonationDateReminderAsync(Guid donorUserId, DateTimeOffset nextAvailableDate);
        Task<ApiResponse<NotificationDto>> CreateBloodRequestStatusNotificationAsync(Guid userId, Guid requestId, string status, bool isEmergency = false);
        
        // Basic appointment notification method
        Task<ApiResponse<NotificationDto>> CreateAppointmentNotificationAsync(Guid userId, Guid appointmentId, string status, DateTimeOffset date, string locationName);
        
        // Enhanced appointment notification method with complete appointment information
        //Task<ApiResponse<NotificationDto>> CreateDetailedAppointmentNotificationAsync(Guid userId, DonationAppointmentRequestDto appointment, string status);
    }
}