using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBloodRequestService
    {
        Task<PagedApiResponse<BloodRequestDto>> GetPagedBloodRequestsAsync(BloodRequestParameters parameters);
        Task<ApiResponse<BloodRequestDto>> GetBloodRequestByIdAsync(Guid id);
        Task<ApiResponse<BloodRequestDto>> CreateBloodRequestAsync(CreateBloodRequestDto bloodRequestDto);
        Task<ApiResponse<BloodRequestDto>> UpdateBloodRequestAsync(Guid id, UpdateBloodRequestDto bloodRequestDto);
        Task<ApiResponse> DeleteBloodRequestAsync(Guid id);
        
        Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetBloodRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, Guid? bloodGroupId = null, string status = null);
        Task<PagedApiResponse<BloodRequestDto>> GetPagedBloodRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, BloodRequestParameters parameters);
    }
}