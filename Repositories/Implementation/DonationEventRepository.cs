using System;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Data;
using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;

namespace Repositories.Implementation
{
    public class DonationEventRepository : GenericRepository<DonationEvent>, IDonationEventRepository
    {
        public DonationEventRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<DonationEvent> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(de => de.DonorProfile)
                .Include(de => de.BloodGroup)
                .Include(de => de.ComponentType)
                .Include(de => de.Location)
                .FirstOrDefaultAsync(de => de.Id == id);
        }

        public async Task<DonationEvent> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(de => de.DonorProfile)
                    .ThenInclude(dp => dp.User)
                .Include(de => de.BloodGroup)
                .Include(de => de.ComponentType)
                .Include(de => de.Location)
                .FirstOrDefaultAsync(de => de.Id == id);
        }

        public async Task<(IEnumerable<DonationEvent> items, int totalCount)> GetPagedDonationEventsAsync(DonationEventParameters parameters)
        {
            IQueryable<DonationEvent> query = _dbSet
                .Include(de => de.DonorProfile)
                    .ThenInclude(dp => dp.User)
                .Include(de => de.BloodGroup)
                .Include(de => de.ComponentType)
                .Include(de => de.Location);

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(de => de.Status.ToLower() == parameters.Status.ToLower());
            }

            if (parameters.DonorId.HasValue)
            {
                query = query.Where(de => de.DonorId == parameters.DonorId);
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(de => de.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(de => de.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.LocationId.HasValue)
            {
                query = query.Where(de => de.LocationId == parameters.LocationId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(de => de.CreatedTime >= parameters.StartDate);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(de => de.CreatedTime <= parameters.EndDate);
            }

            // Apply sorting
            query = ApplySorting(query, parameters);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        private IQueryable<DonationEvent> ApplySorting(IQueryable<DonationEvent> query, DonationEventParameters parameters)
        {
            // Default sort is by creation date, newest first
            if (string.IsNullOrEmpty(parameters.SortBy))
            {
                return parameters.SortAscending
                    ? query.OrderBy(de => de.CreatedTime)
                    : query.OrderByDescending(de => de.CreatedTime);
            }

            // Apply sorting based on the provided field
            return parameters.SortBy.ToLower() switch
            {
                "date" => parameters.SortAscending
                    ? query.OrderBy(de => de.CreatedTime)
                    : query.OrderByDescending(de => de.CreatedTime),
                
                "status" => parameters.SortAscending
                    ? query.OrderBy(de => de.Status)
                    : query.OrderByDescending(de => de.Status),
                
                "quantity" => parameters.SortAscending
                    ? query.OrderBy(de => de.QuantityUnits)
                    : query.OrderByDescending(de => de.QuantityUnits),
                
                "bloodgroup" => parameters.SortAscending
                    ? query.OrderBy(de => de.BloodGroup.GroupName)
                    : query.OrderByDescending(de => de.BloodGroup.GroupName),
                
                "componenttype" => parameters.SortAscending
                    ? query.OrderBy(de => de.ComponentType.Name)
                    : query.OrderByDescending(de => de.ComponentType.Name),
                
                "location" => parameters.SortAscending
                    ? query.OrderBy(de => de.Location.Name)
                    : query.OrderByDescending(de => de.Location.Name),
                
                "donor" => parameters.SortAscending
                    ? query.OrderBy(de => de.DonorProfile.User.FirstName)
                    : query.OrderByDescending(de => de.DonorProfile.User.FirstName),
                
                _ => parameters.SortAscending
                    ? query.OrderBy(de => de.CreatedTime)
                    : query.OrderByDescending(de => de.CreatedTime),
            };
        }
    }
}