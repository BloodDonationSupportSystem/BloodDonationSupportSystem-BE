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
        Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetBloodRequestsByUserIdAsync(Guid userId);
        Task<ApiResponse<BloodRequestDto>> GetBloodRequestByIdAsync(Guid id);
        Task<ApiResponse<BloodRequestDto>> CreateBloodRequestAsync(CreateBloodRequestDto bloodRequestDto);
        Task<ApiResponse<BloodRequestDto>> UpdateBloodRequestAsync(Guid id, UpdateBloodRequestDto bloodRequestDto);
        Task<ApiResponse> DeleteBloodRequestAsync(Guid id);
        Task<ApiResponse<BloodRequestDto>> FulfillBloodRequestFromInventoryAsync(
            Guid requestId,
            FulfillBloodRequestDto fulfillDto);
        Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetBloodRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, Guid? bloodGroupId = null, string status = null);
        Task<PagedApiResponse<BloodRequestDto>> GetPagedBloodRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, BloodRequestParameters parameters);
        
        // Emergency and public request methods
        Task<ApiResponse<BloodRequestDto>> CreatePublicBloodRequestAsync(PublicBloodRequestDto publicRequestDto);
        Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetActiveBloodRequestsAsync(Guid? bloodGroupId = null);
        Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetBloodRequestsByBloodGroupAsync(Guid bloodGroupId, bool onlyActive = true);
        Task<ApiResponse<BloodRequestDto>> UpdateBloodRequestStatusAsync(Guid id, string status);
        Task<ApiResponse<BloodRequestDto>> MarkBloodRequestInactiveAsync(Guid id);
        
        Task<ApiResponse<InventoryCheckResultDto>> CheckInventoryForRequestAsync(Guid requestId);
        Task<ApiResponse<IEnumerable<BloodRequestDto>>> GetEmergencyBloodRequestsAsync(bool onlyActive = true);
    }
}