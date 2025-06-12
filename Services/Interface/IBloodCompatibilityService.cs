using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBloodCompatibilityService
    {
        // Get compatible blood groups for whole blood transfusion
        Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetCompatibleBloodGroupsForWholeBloodAsync(Guid bloodGroupId);
        
        // Get compatible blood groups for specific component transfusion
        Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetCompatibleBloodGroupsForComponentAsync(Guid bloodGroupId, Guid componentTypeId);
        
        // Get compatible donors for whole blood transfusion
        Task<ApiResponse<IEnumerable<DonorProfileDto>>> GetCompatibleDonorsForWholeBloodAsync(Guid bloodGroupId, bool? emergencyOnly = false);
        
        // Get compatible donors for specific component transfusion
        Task<ApiResponse<IEnumerable<DonorProfileDto>>> GetCompatibleDonorsForComponentAsync(Guid bloodGroupId, Guid componentTypeId, bool? emergencyOnly = false);
        
        // Get compatible donor blood group IDs for a specific recipient blood group
        Task<ApiResponse<IEnumerable<Guid>>> GetCompatibleDonorBloodGroupsAsync(Guid recipientBloodGroupId);
    }
}