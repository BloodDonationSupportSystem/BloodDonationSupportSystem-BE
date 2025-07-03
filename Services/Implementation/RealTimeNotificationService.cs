using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Dtos;
using Shared.Hubs;

namespace Services.Implementation
{
    public class RealTimeNotificationService : IRealTimeNotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<RealTimeNotificationService> _logger;

        public RealTimeNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<RealTimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendEmergencyBloodRequestAlert(EmergencyBloodRequestDto emergencyRequest)
        {
            try
            {
                var alertData = new
                {
                    Id = emergencyRequest.Id,
                    PatientName = emergencyRequest.PatientName,
                    BloodGroup = emergencyRequest.BloodGroupName,
                    ComponentType = emergencyRequest.ComponentTypeName,
                    UrgencyLevel = emergencyRequest.UrgencyLevel,
                    HospitalName = emergencyRequest.HospitalName,
                    ContactInfo = emergencyRequest.ContactInfo,
                    QuantityUnits = emergencyRequest.QuantityUnits,
                    Address = emergencyRequest.Address,
                    RequestTime = emergencyRequest.CreatedTime,
                    Type = "EmergencyAlert"
                };

                // Send to Emergency group (high priority staff)
                await _hubContext.Clients.Group("Emergency").SendAsync("EmergencyBloodRequestAlert", alertData);
                
                // Send to Staff group (all staff members)
                await _hubContext.Clients.Group("Staff").SendAsync("NewEmergencyRequest", alertData);

                _logger.LogInformation("Emergency blood request alert sent for request {RequestId}", emergencyRequest.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency blood request alert for request {RequestId}", emergencyRequest.Id);
            }
        }

        public async Task SendEmergencyBloodRequestUpdate(Guid requestId, string status, string message)
        {
            try
            {
                var updateData = new
                {
                    RequestId = requestId,
                    Status = status,
                    Message = message,
                    Timestamp = DateTimeOffset.UtcNow,
                    Type = "EmergencyUpdate"
                };

                await _hubContext.Clients.Group("Emergency").SendAsync("EmergencyRequestUpdate", updateData);
                await _hubContext.Clients.Group("Staff").SendAsync("BloodRequestUpdate", updateData);

                _logger.LogInformation("Emergency request update sent for request {RequestId} with status {Status}", requestId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency request update for request {RequestId}", requestId);
            }
        }

        public async Task NotifyNearbyDonors(EmergencyBloodRequestDto emergencyRequest, List<Guid> donorUserIds)
        {
            try
            {
                var donorAlert = new
                {
                    Id = emergencyRequest.Id,
                    Title = "🚨 URGENT: Emergency Blood Needed",
                    Message = $"Emergency blood request for {emergencyRequest.BloodGroupName} at {emergencyRequest.HospitalName}. " +
                             $"{emergencyRequest.QuantityUnits} units needed urgently.",
                    BloodGroup = emergencyRequest.BloodGroupName,
                    ComponentType = emergencyRequest.ComponentTypeName,
                    UrgencyLevel = emergencyRequest.UrgencyLevel,
                    HospitalName = emergencyRequest.HospitalName,
                    Address = emergencyRequest.Address,
                    ContactInfo = emergencyRequest.ContactInfo,
                    QuantityUnits = emergencyRequest.QuantityUnits,
                    RequestTime = emergencyRequest.CreatedTime,
                    Type = "EmergencyDonorAlert"
                };

                var tasks = donorUserIds.Select(async donorUserId =>
                {
                    await _hubContext.Clients.Group($"User_{donorUserId}").SendAsync("EmergencyDonorAlert", donorAlert);
                });

                await Task.WhenAll(tasks);

                _logger.LogInformation("Emergency donor alerts sent to {DonorCount} nearby donors for request {RequestId}", 
                    donorUserIds.Count, emergencyRequest.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency donor alerts for request {RequestId}", emergencyRequest.Id);
            }
        }

        public async Task NotifyStaffOfNewRequest(BloodRequestDto bloodRequest)
        {
            try
            {
                var requestData = new
                {
                    Id = bloodRequest.Id,
                    Title = bloodRequest.IsEmergency ? "🚨 New Emergency Request" : "📋 New Blood Request",
                    PatientName = bloodRequest.PatientName,
                    RequesterName = bloodRequest.RequesterName,
                    BloodGroup = bloodRequest.BloodGroupName,
                    ComponentType = bloodRequest.ComponentTypeName,
                    QuantityUnits = bloodRequest.QuantityUnits,
                    UrgencyLevel = bloodRequest.UrgencyLevel,
                    IsEmergency = bloodRequest.IsEmergency,
                    Status = bloodRequest.Status,
                    RequestTime = bloodRequest.RequestDate,
                    NeededByDate = bloodRequest.NeededByDate,
                    Address = bloodRequest.Address,
                    HospitalName = bloodRequest.HospitalName,
                    ContactInfo = bloodRequest.ContactInfo,
                    Type = bloodRequest.IsEmergency ? "EmergencyRequest" : "RegularRequest"
                };

                if (bloodRequest.IsEmergency)
                {
                    await _hubContext.Clients.Group("Emergency").SendAsync("NewEmergencyRequest", requestData);
                }

                await _hubContext.Clients.Group("Staff").SendAsync("NewBloodRequest", requestData);

                _logger.LogInformation("Staff notified of new {RequestType} request {RequestId}", 
                    bloodRequest.IsEmergency ? "emergency" : "regular", bloodRequest.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying staff of new request {RequestId}", bloodRequest.Id);
            }
        }

        public async Task NotifyStaffOfRequestUpdate(Guid requestId, string status, string message)
        {
            try
            {
                var updateData = new
                {
                    RequestId = requestId,
                    Status = status,
                    Message = message,
                    Timestamp = DateTimeOffset.UtcNow,
                    Type = "RequestUpdate"
                };

                await _hubContext.Clients.Group("Staff").SendAsync("BloodRequestUpdate", updateData);

                _logger.LogInformation("Staff notified of request update {RequestId} with status {Status}", requestId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying staff of request update {RequestId}", requestId);
            }
        }

        public async Task SendStaffAlert(string title, string message, string urgencyLevel = "Normal")
        {
            try
            {
                var alertData = new
                {
                    Title = title,
                    Message = message,
                    UrgencyLevel = urgencyLevel,
                    Timestamp = DateTimeOffset.UtcNow,
                    Type = "StaffAlert"
                };

                var targetGroup = urgencyLevel == "Critical" ? "Emergency" : "Staff";
                await _hubContext.Clients.Group(targetGroup).SendAsync("StaffAlert", alertData);

                _logger.LogInformation("Staff alert sent: {Title} - {UrgencyLevel}", title, urgencyLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending staff alert: {Title}", title);
            }
        }

        public async Task NotifyDonor(Guid donorUserId, string title, string message, string type)
        {
            try
            {
                var notificationData = new
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTimeOffset.UtcNow
                };

                await _hubContext.Clients.Group($"User_{donorUserId}").SendAsync("DonorNotification", notificationData);

                _logger.LogInformation("Notification sent to donor {DonorUserId}: {Title}", donorUserId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to donor {DonorUserId}", donorUserId);
            }
        }

        public async Task NotifyMultipleDonors(List<Guid> donorUserIds, string title, string message, string type)
        {
            try
            {
                var notificationData = new
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTimeOffset.UtcNow
                };

                var tasks = donorUserIds.Select(async donorUserId =>
                {
                    await _hubContext.Clients.Group($"User_{donorUserId}").SendAsync("DonorNotification", notificationData);
                });

                await Task.WhenAll(tasks);

                _logger.LogInformation("Notifications sent to {DonorCount} donors: {Title}", donorUserIds.Count, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications to multiple donors");
            }
        }

        public async Task BroadcastSystemNotification(string title, string message, string type)
        {
            try
            {
                var notificationData = new
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Timestamp = DateTimeOffset.UtcNow
                };

                await _hubContext.Clients.All.SendAsync("SystemNotification", notificationData);

                _logger.LogInformation("System notification broadcasted: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting system notification: {Title}", title);
            }
        }

        public async Task SendToUserGroup(string userGroupName, string title, string message, object? data = null)
        {
            try
            {
                var notificationData = new
                {
                    Title = title,
                    Message = message,
                    Data = data,
                    Timestamp = DateTimeOffset.UtcNow
                };

                await _hubContext.Clients.Group(userGroupName).SendAsync("GroupNotification", notificationData);

                _logger.LogInformation("Notification sent to group {GroupName}: {Title}", userGroupName, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to group {GroupName}", userGroupName);
            }
        }

        public async Task SendToUser(Guid userId, string title, string message, object? data = null)
        {
            try
            {
                var notificationData = new
                {
                    Title = title,
                    Message = message,
                    Data = data,
                    Timestamp = DateTimeOffset.UtcNow
                };

                await _hubContext.Clients.Group($"User_{userId}").SendAsync("UserNotification", notificationData);

                _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        public async Task NotifyBloodRequestStatusChange(Guid requestId, string status, string details)
        {
            try
            {
                var statusData = new
                {
                    RequestId = requestId,
                    Status = status,
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    Type = "StatusChange"
                };

                await _hubContext.Clients.Group("Staff").SendAsync("BloodRequestStatusChange", statusData);

                _logger.LogInformation("Blood request status change notification sent for {RequestId}: {Status}", requestId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending blood request status change notification for {RequestId}", requestId);
            }
        }

        public async Task NotifyDonationEventStatusChange(Guid donationEventId, string status, string details)
        {
            try
            {
                var statusData = new
                {
                    DonationEventId = donationEventId,
                    Status = status,
                    Details = details,
                    Timestamp = DateTimeOffset.UtcNow,
                    Type = "DonationEventStatusChange"
                };

                await _hubContext.Clients.Group("Staff").SendAsync("DonationEventStatusChange", statusData);

                _logger.LogInformation("Donation event status change notification sent for {DonationEventId}: {Status}", donationEventId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending donation event status change notification for {DonationEventId}", donationEventId);
            }
        }

        public async Task UpdateEmergencyDashboard()
        {
            try
            {
                var dashboardUpdate = new
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    Type = "EmergencyDashboardUpdate"
                };

                await _hubContext.Clients.Group("Emergency").SendAsync("UpdateEmergencyDashboard", dashboardUpdate);
                await _hubContext.Clients.Group("Staff").SendAsync("UpdateEmergencyDashboard", dashboardUpdate);

                _logger.LogInformation("Emergency dashboard update notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending emergency dashboard update notification");
            }
        }

        public async Task UpdateBloodRequestDashboard()
        {
            try
            {
                var dashboardUpdate = new
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    Type = "BloodRequestDashboardUpdate"
                };

                await _hubContext.Clients.Group("Staff").SendAsync("UpdateBloodRequestDashboard", dashboardUpdate);

                _logger.LogInformation("Blood request dashboard update notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending blood request dashboard update notification");
            }
        }

        public async Task UpdateInventoryDashboard()
        {
            try
            {
                var dashboardUpdate = new
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    Type = "InventoryDashboardUpdate"
                };

                await _hubContext.Clients.Group("Staff").SendAsync("UpdateInventoryDashboard", dashboardUpdate);

                _logger.LogInformation("Inventory dashboard update notification sent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending inventory dashboard update notification");
            }
        }

        public async Task<int> GetOnlineStaffCount()
        {
            // This is a simplified implementation
            // In a real application, you might want to track online users in a database or cache
            return 0;
        }

        public async Task<int> GetOnlineDonorCount()
        {
            // This is a simplified implementation
            // In a real application, you might want to track online users in a database or cache
            return 0;
        }

        public async Task<bool> IsUserOnline(Guid userId)
        {
            // This is a simplified implementation
            // In a real application, you might want to track online users in a database or cache
            return false;
        }
    }
}