using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IRequestMatchRepository : IGenericRepository<RequestMatch>
    {
        Task<RequestMatch> GetByIdWithDetailsAsync(Guid id);
        Task<IEnumerable<RequestMatch>> GetByRequestIdAsync(Guid requestId);
        Task<IEnumerable<RequestMatch>> GetByEmergencyRequestIdAsync(Guid emergencyRequestId);
        Task<IEnumerable<RequestMatch>> GetByDonationEventIdAsync(Guid donationEventId);
        Task<(IEnumerable<RequestMatch> requestMatches, int totalCount)> GetPagedRequestMatchesAsync(RequestMatchParameters parameters);
    }
}