using BusinessObjects.Data;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class DonorProfileRepository : GenericRepository<DonorProfile>, IDonorProfileRepository
    {
        public DonorProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DonorProfile> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .FirstOrDefaultAsync(dp => dp.Id == id);
        }

        public async Task<DonorProfile> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .FirstOrDefaultAsync(dp => dp.UserId == userId);
        }

        public async Task<IEnumerable<DonorProfile>> GetByBloodGroupIdAsync(Guid bloodGroupId)
        {
            return await _dbSet
                .Include(dp => dp.User)
                .Include(dp => dp.BloodGroup)
                .Where(dp => dp.BloodGroupId == bloodGroupId)
                .ToListAsync();
        }

        public async Task<bool> IsUserAlreadyDonorAsync(Guid userId)
        {
            return await _dbSet.AnyAsync(dp => dp.UserId == userId);
        }

        public async Task<(IEnumerable<DonorProfile> donorProfiles, int totalCount)> GetPagedDonorProfilesAsync(DonorProfileParameters parameters)
        {
            var query = _dbSet.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.BloodGroup))
            {
                query = query.Where(dp => dp.BloodGroup.GroupName.Contains(parameters.BloodGroup));
            }

            if (!string.IsNullOrEmpty(parameters.HealthStatus))
            {
                query = query.Where(dp => dp.HealthStatus.Contains(parameters.HealthStatus));
            }

            if (parameters.MinimumDonations.HasValue)
            {
                query = query.Where(dp => dp.TotalDonations >= parameters.MinimumDonations.Value);
            }

            // Filter soft deleted
            query = query.Where(dp => dp.DeletedTime == null);

            // Get total count
            var totalCount = await query.CountAsync();

            // Include related entities
            query = query.Include(dp => dp.User).Include(dp => dp.BloodGroup);

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "name" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.User.FirstName).ThenBy(dp => dp.User.LastName) 
                    : query.OrderByDescending(dp => dp.User.FirstName).ThenByDescending(dp => dp.User.LastName),
                "bloodgroup" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.BloodGroup.GroupName) 
                    : query.OrderByDescending(dp => dp.BloodGroup.GroupName),
                "donations" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.TotalDonations) 
                    : query.OrderByDescending(dp => dp.TotalDonations),
                "lastdonation" => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.LastDonationDate) 
                    : query.OrderByDescending(dp => dp.LastDonationDate),
                _ => parameters.SortAscending 
                    ? query.OrderBy(dp => dp.CreatedTime) 
                    : query.OrderByDescending(dp => dp.CreatedTime)
            };

            // Apply pagination
            var donorProfiles = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (donorProfiles, totalCount);
        }
    }
}