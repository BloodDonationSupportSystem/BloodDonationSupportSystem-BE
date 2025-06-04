using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IDonorProfileService
    {
        Task<ApiResponse<IEnumerable<DonorProfileDto>>> GetAllDonorProfilesAsync();
        Task<ApiResponse<DonorProfileDto>> GetDonorProfileByIdAsync(Guid id);
        Task<ApiResponse<DonorProfileDto>> GetDonorProfileByUserIdAsync(Guid userId);
        Task<ApiResponse<IEnumerable<DonorProfileDto>>> GetDonorProfilesByBloodGroupIdAsync(Guid bloodGroupId);
        Task<ApiResponse<DonorProfileDto>> CreateDonorProfileAsync(CreateDonorProfileDto donorProfileDto);
        Task<ApiResponse<DonorProfileDto>> UpdateDonorProfileAsync(Guid id, UpdateDonorProfileDto donorProfileDto);
        Task<ApiResponse> DeleteDonorProfileAsync(Guid id);
        Task<PagedApiResponse<DonorProfileDto>> GetPagedDonorProfilesAsync(DonorProfileParameters parameters);
    }
}