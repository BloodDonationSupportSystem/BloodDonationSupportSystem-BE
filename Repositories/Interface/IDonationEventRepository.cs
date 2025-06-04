using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IDonationEventRepository : IGenericRepository<DonationEvent>
    {
        Task<(IEnumerable<DonationEvent> items, int totalCount)> GetPagedDonationEventsAsync(DonationEventParameters parameters);
        Task<DonationEvent> GetByIdWithDetailsAsync(Guid id);
    }
}