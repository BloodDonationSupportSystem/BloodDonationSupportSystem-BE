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
    }
}