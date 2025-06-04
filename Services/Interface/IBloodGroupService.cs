using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBloodGroupService
    {
        Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetAllBloodGroupsAsync();
        Task<ApiResponse<BloodGroupDto>> GetBloodGroupByIdAsync(Guid id);
        Task<ApiResponse<BloodGroupDto>> CreateBloodGroupAsync(CreateBloodGroupDto bloodGroupDto);
        Task<ApiResponse<BloodGroupDto>> UpdateBloodGroupAsync(Guid id, UpdateBloodGroupDto bloodGroupDto);
        Task<ApiResponse> DeleteBloodGroupAsync(Guid id);
    }
}