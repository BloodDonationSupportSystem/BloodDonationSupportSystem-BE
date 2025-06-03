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
    public class BloodRequestRepository : GenericRepository<BloodRequest>, IBloodRequestRepository
    {
        public BloodRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<BloodRequest> items, int totalCount)> GetPagedBloodRequestsAsync(BloodRequestParameters parameters)
        {
            IQueryable<BloodRequest> query = _dbSet
                .Include(br => br.User)
                .Include(br => br.BloodGroup)
                .Include(br => br.ComponentType)
                .Include(br => br.Location);

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(br => br.Status.ToLower() == parameters.Status.ToLower());
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(br => br.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(br => br.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.LocationId.HasValue)
            {
                query = query.Where(br => br.LocationId == parameters.LocationId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(br => br.RequestDate >= parameters.StartDate);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(br => br.RequestDate <= parameters.EndDate);
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

        private IQueryable<BloodRequest> ApplySorting(IQueryable<BloodRequest> query, BloodRequestParameters parameters)
        {
            // Default sort is by request date, newest first
            if (string.IsNullOrEmpty(parameters.SortBy))
            {
                return parameters.SortAscending
                    ? query.OrderBy(br => br.RequestDate)
                    : query.OrderByDescending(br => br.RequestDate);
            }

            // Apply sorting based on the provided field
            return parameters.SortBy.ToLower() switch
            {
                "date" => parameters.SortAscending
                    ? query.OrderBy(br => br.RequestDate)
                    : query.OrderByDescending(br => br.RequestDate),
                
                "neededby" => parameters.SortAscending
                    ? query.OrderBy(br => br.NeededByDate)
                    : query.OrderByDescending(br => br.NeededByDate),
                
                "status" => parameters.SortAscending
                    ? query.OrderBy(br => br.Status)
                    : query.OrderByDescending(br => br.Status),
                
                "quantity" => parameters.SortAscending
                    ? query.OrderBy(br => br.QuantityUnits)
                    : query.OrderByDescending(br => br.QuantityUnits),
                
                "bloodgroup" => parameters.SortAscending
                    ? query.OrderBy(br => br.BloodGroup.GroupName)
                    : query.OrderByDescending(br => br.BloodGroup.GroupName),
                
                "location" => parameters.SortAscending
                    ? query.OrderBy(br => br.Location.Name)
                    : query.OrderByDescending(br => br.Location.Name),
                
                _ => parameters.SortAscending
                    ? query.OrderBy(br => br.RequestDate)
                    : query.OrderByDescending(br => br.RequestDate),
            };
        }
    }
}