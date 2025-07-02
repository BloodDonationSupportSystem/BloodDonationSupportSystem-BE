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
        
        // Enhanced notification methods for specific scenarios
        Task<ApiResponse<NotificationDto>> CreateDonorRestReminderAsync(Guid donorUserId, DateTimeOffset donationDate);
        Task<ApiResponse<NotificationDto>> CreateNextDonationDateReminderAsync(Guid donorUserId, DateTimeOffset nextAvailableDate);
        Task<ApiResponse<NotificationDto>> CreateBloodRequestStatusNotificationAsync(Guid userId, Guid requestId, string status, bool isEmergency = false);
        
        // Enhanced appointment notification methods with complete appointment information
        Task<ApiResponse<NotificationDto>> CreateAppointmentNotificationAsync(Guid userId, Guid appointmentId, string status, DateTimeOffset date, string locationName);
        Task<ApiResponse<NotificationDto>> CreateDetailedAppointmentNotificationAsync(Guid userId, DonationAppointmentRequestDto appointment, string status, string additionalInfo = null);
        
        // Enhanced donation event notification methods
        Task<ApiResponse<NotificationDto>> CreateDetailedDonationEventNotificationAsync(Guid userId, DonationEventDto donationEvent, string status, string additionalInfo = null);
        Task<ApiResponse<NotificationDto>> CreateDonationProcessNotificationAsync(Guid userId, DonationEventDto donationEvent, string processStep, string details = null);
        
        // Comprehensive blood request notification methods
        Task<ApiResponse<NotificationDto>> CreateDetailedBloodRequestNotificationAsync(Guid userId, Guid requestId, string requestType, string status, string bloodType, string location, string additionalDetails = null);
        
        // Emergency notification methods
        Task<ApiResponse<NotificationDto>> CreateEmergencyNotificationAsync(Guid userId, string emergencyType, string details, string urgencyLevel = "High");
        
        // System and administrative notifications
        Task<ApiResponse<NotificationDto>> CreateSystemNotificationAsync(Guid userId, string notificationType, string title, string message, string priority = "Normal");
        
        // Bulk notification methods
        Task<ApiResponse<List<NotificationDto>>> CreateBulkNotificationsAsync(List<Guid> userIds, string title, string type, string message);
        Task<ApiResponse<List<NotificationDto>>> CreateRoleBasedNotificationAsync(string roleName, string title, string type, string message);
        
        // Scheduled notification methods
        Task<ApiResponse<NotificationDto>> ScheduleNotificationAsync(CreateNotificationDto notificationDto, DateTimeOffset scheduleTime);
        Task<ApiResponse> CancelScheduledNotificationAsync(Guid notificationId);
        
        // Notification preference methods
        Task<ApiResponse<bool>> CheckUserNotificationPreferencesAsync(Guid userId, string notificationType);
        Task<ApiResponse<NotificationDto>> CreateNotificationWithPreferencesAsync(CreateNotificationDto notificationDto, bool forceEmail = false);
    }
}