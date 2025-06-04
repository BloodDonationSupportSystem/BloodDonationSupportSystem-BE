using System;
using System.Threading.Tasks;
using Repositories.Interface;

namespace Repositories.Base
{
    public interface IUnitOfWork : IDisposable
    {
        ILocationRepository Locations { get; }
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
        IRequestMatchRepository RequestMatches { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        
        Task<int> CompleteAsync();
    }
}