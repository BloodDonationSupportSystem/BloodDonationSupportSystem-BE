using System;
using System.Threading.Tasks;
using Repositories.Interface;

namespace Repositories.Base
{
    public interface IUnitOfWork : IDisposable
    {
        ILocationRepository Locations { get; }
        //IBloodRequestRepository BloodRequests { get; }
        //IBloodGroupRepository BloodGroups { get; }
        //IComponentTypeRepository ComponentTypes { get; }
        //IUserRepository Users { get; }
        
        Task<int> CompleteAsync();
    }
}