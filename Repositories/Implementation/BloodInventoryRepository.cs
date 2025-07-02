using System;
using System.Collections.Generic;
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
    public class BloodInventoryRepository : GenericRepository<BloodInventory>, IBloodInventoryRepository
    {
        public BloodInventoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BloodInventory> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(bi => bi.BloodGroup)
                .Include(bi => bi.ComponentType)
                .Include(bi => bi.DonationEvent)
                    .ThenInclude(de => de.DonorProfile)
                        .ThenInclude(dp => dp.User)
                .Include(bi => bi.DonationEvent)
                    .ThenInclude(de => de.Location)
                .FirstOrDefaultAsync(bi => bi.Id == id);
        }

        public async Task<(IEnumerable<BloodInventory> items, int totalCount)> GetPagedBloodInventoriesAsync(BloodInventoryParameters parameters)
        {
            IQueryable<BloodInventory> query = _dbSet
                .Include(bi => bi.BloodGroup)
                .Include(bi => bi.ComponentType)
                .Include(bi => bi.DonationEvent)
                    .ThenInclude(de => de.DonorProfile)
                        .ThenInclude(dp => dp.User)
                .Include(bi => bi.DonationEvent)
                    .ThenInclude(de => de.Location);

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(bi => bi.Status.ToLower() == parameters.Status.ToLower());
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(bi => bi.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(bi => bi.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.ExpirationStartDate.HasValue)
            {
                query = query.Where(bi => bi.ExpirationDate >= parameters.ExpirationStartDate);
            }

            if (parameters.ExpirationEndDate.HasValue)
            {
                query = query.Where(bi => bi.ExpirationDate <= parameters.ExpirationEndDate);
            }

            if (parameters.IsExpired.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                if (parameters.IsExpired.Value)
                {
                    query = query.Where(bi => bi.ExpirationDate < now);
                }
                else
                {
                    query = query.Where(bi => bi.ExpirationDate >= now);
                }
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

        public async Task<IEnumerable<BloodInventory>> GetExpiredInventoryAsync()
        {
            var now = DateTimeOffset.UtcNow;
            return await _dbSet
                .Include(bi => bi.BloodGroup)
                .Include(bi => bi.ComponentType)
                .Include(bi => bi.DonationEvent)
                    .ThenInclude(de => de.DonorProfile)
                        .ThenInclude(dp => dp.User)
                .Include(bi => bi.DonationEvent)
                    .ThenInclude(de => de.Location)
                .Where(bi => bi.ExpirationDate < now && bi.Status == "Available")
                .ToListAsync();
        }

        public async Task<IEnumerable<BloodInventory>> GetByBloodGroupAndComponentTypeAsync(Guid bloodGroupId, Guid componentTypeId)
        {
            return await _dbSet
                .Include(bi => bi.BloodGroup)
                .Include(bi => bi.ComponentType)
                .Include(bi => bi.DonationEvent)
                    .ThenInclude(de => de.DonorProfile)
                        .ThenInclude(dp => dp.User)
                .Include(bi => bi.DonationEvent)
                    .ThenInclude(de => de.Location)
                .Where(bi => bi.BloodGroupId == bloodGroupId && 
                             bi.ComponentTypeId == componentTypeId && 
                             bi.Status == "Available" && 
                             bi.ExpirationDate > DateTimeOffset.UtcNow)
                .ToListAsync();
        }

        public async Task<int> GetAvailableQuantityAsync(Guid bloodGroupId, Guid componentTypeId)
        {
            return await _dbSet
                .Where(bi => bi.BloodGroupId == bloodGroupId && 
                             bi.ComponentTypeId == componentTypeId && 
                             bi.Status == "Available" && 
                             bi.ExpirationDate > DateTimeOffset.UtcNow)
                .SumAsync(bi => bi.QuantityUnits);
        }

        private IQueryable<BloodInventory> ApplySorting(IQueryable<BloodInventory> query, BloodInventoryParameters parameters)
        {
            // Default sort is by expiration date, soonest first
            if (string.IsNullOrEmpty(parameters.SortBy))
            {
                return parameters.SortAscending
                    ? query.OrderBy(bi => bi.ExpirationDate)
                    : query.OrderByDescending(bi => bi.ExpirationDate);
            }

            // Apply sorting based on the provided field
            return parameters.SortBy.ToLower() switch
            {
                "expiration" => parameters.SortAscending
                    ? query.OrderBy(bi => bi.ExpirationDate)
                    : query.OrderByDescending(bi => bi.ExpirationDate),
                
                "quantity" => parameters.SortAscending
                    ? query.OrderBy(bi => bi.QuantityUnits)
                    : query.OrderByDescending(bi => bi.QuantityUnits),
                
                "status" => parameters.SortAscending
                    ? query.OrderBy(bi => bi.Status)
                    : query.OrderByDescending(bi => bi.Status),
                
                "bloodgroup" => parameters.SortAscending
                    ? query.OrderBy(bi => bi.BloodGroup.GroupName)
                    : query.OrderByDescending(bi => bi.BloodGroup.GroupName),
                
                "componenttype" => parameters.SortAscending
                    ? query.OrderBy(bi => bi.ComponentType.Name)
                    : query.OrderByDescending(bi => bi.ComponentType.Name),
                
                "donationdate" => parameters.SortAscending
                    ? query.OrderBy(bi => bi.DonationEvent.DonationDate)
                    : query.OrderByDescending(bi => bi.DonationEvent.DonationDate),
                
                _ => parameters.SortAscending
                    ? query.OrderBy(bi => bi.ExpirationDate)
                    : query.OrderByDescending(bi => bi.ExpirationDate),
            };
        }
    }
}