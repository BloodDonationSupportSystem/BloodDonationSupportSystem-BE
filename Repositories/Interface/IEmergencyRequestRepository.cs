using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IEmergencyRequestRepository : IGenericRepository<EmergencyRequest>
    {
        Task<(IEnumerable<EmergencyRequest> items, int totalCount)> GetPagedEmergencyRequestsAsync(EmergencyRequestParameters parameters);
        Task<EmergencyRequest> GetByIdWithDetailsAsync(Guid id);
    }
}