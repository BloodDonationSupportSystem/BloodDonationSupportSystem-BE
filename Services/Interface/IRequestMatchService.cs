using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IRequestMatchService
    {
        Task<ApiResponse<IEnumerable<RequestMatchDto>>> GetAllRequestMatchesAsync();
        Task<ApiResponse<RequestMatchDto>> GetRequestMatchByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<RequestMatchDto>>> GetRequestMatchesByRequestIdAsync(Guid requestId);
        Task<ApiResponse<IEnumerable<RequestMatchDto>>> GetRequestMatchesByEmergencyRequestIdAsync(Guid emergencyRequestId);
        Task<ApiResponse<IEnumerable<RequestMatchDto>>> GetRequestMatchesByDonationEventIdAsync(Guid donationEventId);
        Task<ApiResponse<RequestMatchDto>> CreateRequestMatchAsync(CreateRequestMatchDto requestMatchDto);
        Task<ApiResponse<RequestMatchDto>> UpdateRequestMatchAsync(Guid id, UpdateRequestMatchDto requestMatchDto);
        Task<ApiResponse> DeleteRequestMatchAsync(Guid id);
        Task<PagedApiResponse<RequestMatchDto>> GetPagedRequestMatchesAsync(RequestMatchParameters parameters);
    }
}