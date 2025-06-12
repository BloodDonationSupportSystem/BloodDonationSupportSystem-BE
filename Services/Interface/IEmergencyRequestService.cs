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
        
        // New methods for location-based searches
        Task<ApiResponse<IEnumerable<EmergencyRequestDto>>> GetEmergencyRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, Guid? bloodGroupId = null, string urgencyLevel = null, bool? isActive = null);
        Task<PagedApiResponse<EmergencyRequestDto>> GetPagedEmergencyRequestsByDistanceAsync(double latitude, double longitude, double radiusKm, EmergencyRequestParameters parameters);
        
        // Method for public emergency request creation (without authentication)
        Task<ApiResponse<EmergencyRequestDto>> CreatePublicEmergencyRequestAsync(PublicEmergencyRequestDto publicRequestDto);
        
        // Get active emergency requests
        Task<ApiResponse<IEnumerable<EmergencyRequestDto>>> GetActiveEmergencyRequestsAsync(Guid? bloodGroupId = null);
        
        // Get emergency requests by blood group
        Task<ApiResponse<IEnumerable<EmergencyRequestDto>>> GetEmergencyRequestsByBloodGroupAsync(Guid bloodGroupId, bool onlyActive = true);
        
        // Update emergency request status
        Task<ApiResponse<EmergencyRequestDto>> UpdateEmergencyRequestStatusAsync(Guid id, string status);
        
        // Mark emergency request as inactive (fulfilled or no longer needed)
        Task<ApiResponse<EmergencyRequestDto>> MarkEmergencyRequestInactiveAsync(Guid id);
    }
}