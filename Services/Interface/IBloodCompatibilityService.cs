using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBloodCompatibilityService
    {
        /// <summary>
        /// Gets all compatible blood groups for a recipient with the specified blood group
        /// </summary>
        /// <param name="recipientBloodGroupId">The blood group ID of the recipient</param>
        /// <returns>A list of compatible blood groups for whole blood transfusion</returns>
        Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetCompatibleWholeBloodGroupsAsync(Guid recipientBloodGroupId);
        
        /// <summary>
        /// Gets all compatible blood groups for a specific blood component
        /// </summary>
        /// <param name="recipientBloodGroupId">The blood group ID of the recipient</param>
        /// <param name="componentTypeId">The component type ID</param>
        /// <returns>A list of compatible blood groups for the specified component</returns>
        Task<ApiResponse<IEnumerable<BloodGroupDto>>> GetCompatibleComponentBloodGroupsAsync(Guid recipientBloodGroupId, Guid componentTypeId);
        
        /// <summary>
        /// Gets detailed compatibility information for all blood groups
        /// </summary>
        /// <returns>A list of blood group compatibility details</returns>
        Task<ApiResponse<IEnumerable<BloodGroupCompatibilityDto>>> GetBloodGroupCompatibilityMatrixAsync();
        
        /// <summary>
        /// Gets compatible donor blood group IDs for a specific recipient blood group
        /// </summary>
        /// <param name="recipientBloodGroupId">The blood group ID of the recipient</param>
        /// <returns>A list of compatible donor blood group IDs</returns>
        Task<ApiResponse<IEnumerable<Guid>>> GetCompatibleDonorBloodGroupsAsync(Guid recipientBloodGroupId);
    }
}