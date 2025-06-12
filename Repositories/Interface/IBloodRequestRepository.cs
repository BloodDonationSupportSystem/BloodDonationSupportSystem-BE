using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IBloodRequestRepository : IGenericRepository<BloodRequest>
    {
        Task<(IEnumerable<BloodRequest> items, int totalCount)> GetPagedBloodRequestsAsync(BloodRequestParameters parameters);
        
        // New methods for distance-based searches
        Task<IEnumerable<BloodRequest>> GetBloodRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, Guid? bloodGroupId = null, string status = null);
        Task<(IEnumerable<BloodRequest> requests, int totalCount)> GetPagedBloodRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, BloodRequestParameters parameters);
    }
}