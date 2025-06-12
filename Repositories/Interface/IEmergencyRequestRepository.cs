using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IEmergencyRequestRepository : IGenericRepository<EmergencyRequest>
    {
        Task<(IEnumerable<EmergencyRequest> items, int totalCount)> GetPagedEmergencyRequestsAsync(EmergencyRequestParameters parameters);
        Task<EmergencyRequest> GetByIdWithDetailsAsync(Guid id);
        
        // Methods for distance-based searches
        Task<IEnumerable<EmergencyRequest>> GetEmergencyRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, Guid? bloodGroupId = null, string urgencyLevel = null, bool? isActive = null);
        Task<(IEnumerable<EmergencyRequest> requests, int totalCount)> GetPagedEmergencyRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, EmergencyRequestParameters parameters);
        
        // Get active emergency requests
        Task<IEnumerable<EmergencyRequest>> GetActiveEmergencyRequestsAsync(Guid? bloodGroupId = null);
        
        // Get emergency requests by blood group
        Task<IEnumerable<EmergencyRequest>> GetEmergencyRequestsByBloodGroupAsync(Guid bloodGroupId, bool onlyActive = true);
    }
}