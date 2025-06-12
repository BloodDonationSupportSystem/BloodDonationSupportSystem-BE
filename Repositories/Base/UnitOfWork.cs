using BusinessObjects.Data;
using Repositories.Implementation;
using Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace Repositories.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ILocationRepository _locationRepository;
        private IBloodGroupRepository _bloodGroupRepository;
        private IComponentTypeRepository _componentTypeRepository;
        private IRoleRepository _roleRepository;
        private IUserRepository _userRepository;
        private IBloodRequestRepository _bloodRequestRepository;
        private IDonationEventRepository _donationEventRepository;
        private IEmergencyRequestRepository _emergencyRequestRepository;
        private IBloodInventoryRepository _bloodInventoryRepository;
        private IBlogPostRepository _blogPostRepository;
        private IDocumentRepository _documentRepository;
        private IDonorProfileRepository _donorProfileRepository;
        private INotificationRepository _notificationRepository;
        private IRefreshTokenRepository _refreshTokenRepository;
        private IBloodDonationWorkflowRepository _bloodDonationWorkflowRepository;
        private IDonorReminderSettingsRepository _donorReminderSettingsRepository;
        private IAnalyticsRepository _analyticsRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ILocationRepository Locations => _locationRepository ??= new LocationRepository(_context);
        public IBloodGroupRepository BloodGroups => _bloodGroupRepository ??= new BloodGroupRepository(_context);
        public IComponentTypeRepository ComponentTypes => _componentTypeRepository ??= new ComponentTypeRepository(_context);
        public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);
        public IUserRepository Users => _userRepository ??= new UserRepository(_context);
        public IBloodRequestRepository BloodRequests => _bloodRequestRepository ??= new BloodRequestRepository(_context);
        public IDonationEventRepository DonationEvents => _donationEventRepository ??= new DonationEventRepository(_context);
        public IEmergencyRequestRepository EmergencyRequests => _emergencyRequestRepository ??= new EmergencyRequestRepository(_context);
        public IBloodInventoryRepository BloodInventories => _bloodInventoryRepository ??= new BloodInventoryRepository(_context);
        public IBlogPostRepository BlogPosts => _blogPostRepository ??= new BlogPostRepository(_context);
        public IDocumentRepository Documents => _documentRepository ??= new DocumentRepository(_context);
        public IDonorProfileRepository DonorProfiles => _donorProfileRepository ??= new DonorProfileRepository(_context);
        public INotificationRepository Notifications => _notificationRepository ??= new NotificationRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository ??= new RefreshTokenRepository(_context);
        public IBloodDonationWorkflowRepository BloodDonationWorkflows => _bloodDonationWorkflowRepository ??= new BloodDonationWorkflowRepository(_context);
        public IDonorReminderSettingsRepository DonorReminderSettings => _donorReminderSettingsRepository ??= new DonorReminderSettingsRepository(_context);
        public IAnalyticsRepository Analytics => _analyticsRepository ??= new AnalyticsRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}