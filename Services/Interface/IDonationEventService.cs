using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IDonationEventService
    {
        Task<PagedApiResponse<DonationEventDto>> GetPagedDonationEventsAsync(DonationEventParameters parameters);
        Task<ApiResponse<DonationEventDto>> GetDonationEventByIdAsync(Guid id);
        Task<ApiResponse<DonationEventDto>> CreateDonationEventAsync(CreateDonationEventDto donationEventDto);
        Task<ApiResponse<DonationEventDto>> UpdateDonationEventAsync(Guid id, UpdateDonationEventDto donationEventDto);
        Task<ApiResponse> DeleteDonationEventAsync(Guid id);
    }
}