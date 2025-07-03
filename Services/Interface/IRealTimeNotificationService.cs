using BusinessObjects.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IRealTimeNotificationService
    {
        // Emergency blood request notifications
        Task SendEmergencyBloodRequestAlert(EmergencyBloodRequestDto emergencyRequest);
        Task SendEmergencyBloodRequestUpdate(Guid requestId, string status, string message);
        Task NotifyNearbyDonors(EmergencyBloodRequestDto emergencyRequest, List<Guid> donorUserIds);

        // Staff notifications
        Task NotifyStaffOfNewRequest(BloodRequestDto bloodRequest);
        Task NotifyStaffOfRequestUpdate(Guid requestId, string status, string message);
        Task SendStaffAlert(string title, string message, string urgencyLevel = "Normal");

        // Donor notifications
        Task NotifyDonor(Guid donorUserId, string title, string message, string type);
        Task NotifyMultipleDonors(List<Guid> donorUserIds, string title, string message, string type);

        // General notifications
        Task BroadcastSystemNotification(string title, string message, string type);
        Task SendToUserGroup(string userGroupName, string title, string message, object? data = null);
        Task SendToUser(Guid userId, string title, string message, object? data = null);

        // Status change notifications
        Task NotifyBloodRequestStatusChange(Guid requestId, string status, string message);
        Task NotifyDonationEventStatusChange(Guid donationEventId, string status, string message);

        // Dashboard updates
        Task UpdateEmergencyDashboard();
        Task UpdateBloodRequestDashboard();
        Task UpdateInventoryDashboard();

        // Connection management
        Task<int> GetOnlineStaffCount();
        Task<int> GetOnlineDonorCount();
        Task<bool> IsUserOnline(Guid userId);
    }
}