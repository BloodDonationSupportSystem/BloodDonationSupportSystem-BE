using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IBloodRequestRepository : IGenericRepository<BloodRequest>
    {
        Task<(IEnumerable<BloodRequest> items, int totalCount)> GetPagedBloodRequestsAsync(BloodRequestParameters parameters);
    }
}