using System;
using System.Threading.Tasks;
using Repositories.Interface;
using System.Collections.Generic;

namespace Repositories.Base
{
    public interface IUnitOfWork : IDisposable
    {
        ILocationRepository Locations { get; }
        ILocationCapacityRepository LocationCapacities { get; }
        ILocationStaffAssignmentRepository LocationStaffAssignments { get; }
        IBloodGroupRepository BloodGroups { get; }
        IComponentTypeRepository ComponentTypes { get; }
        IRoleRepository Roles { get; }
        IUserRepository Users { get; }
        IBloodRequestRepository BloodRequests { get; }
        IDonationEventRepository DonationEvents { get; }
        IEmergencyRequestRepository EmergencyRequests { get; }
        IBloodInventoryRepository BloodInventories { get; }
        IBlogPostRepository BlogPosts { get; }
        IDocumentRepository Documents { get; }
        IDonorProfileRepository DonorProfiles { get; }
        INotificationRepository Notifications { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        IDonorReminderSettingsRepository DonorReminderSettings { get; }
        IAnalyticsRepository Analytics { get; }
        IDonationAppointmentRequestRepository DonationAppointmentRequests { get; }
        
        Task<int> CompleteAsync();
    }
}