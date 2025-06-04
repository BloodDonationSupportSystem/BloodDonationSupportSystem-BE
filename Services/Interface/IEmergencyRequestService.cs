using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IEmergencyRequestService
    {
        Task<PagedApiResponse<EmergencyRequestDto>> GetPagedEmergencyRequestsAsync(EmergencyRequestParameters parameters);
        Task<ApiResponse<EmergencyRequestDto>> GetEmergencyRequestByIdAsync(Guid id);
        Task<ApiResponse<EmergencyRequestDto>> CreateEmergencyRequestAsync(CreateEmergencyRequestDto emergencyRequestDto);
        Task<ApiResponse<EmergencyRequestDto>> UpdateEmergencyRequestAsync(Guid id, UpdateEmergencyRequestDto emergencyRequestDto);
        Task<ApiResponse> DeleteEmergencyRequestAsync(Guid id);
    }
}