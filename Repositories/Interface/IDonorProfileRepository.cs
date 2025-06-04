using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IDonorProfileRepository : IGenericRepository<DonorProfile>
    {
        Task<DonorProfile> GetByIdWithDetailsAsync(Guid id);
        Task<DonorProfile> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<DonorProfile>> GetByBloodGroupIdAsync(Guid bloodGroupId);
        Task<bool> IsUserAlreadyDonorAsync(Guid userId);
        Task<(IEnumerable<DonorProfile> donorProfiles, int totalCount)> GetPagedDonorProfilesAsync(DonorProfileParameters parameters);
    }
}