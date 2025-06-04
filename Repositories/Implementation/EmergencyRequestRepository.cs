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
    public class EmergencyRequestRepository : GenericRepository<EmergencyRequest>, IEmergencyRequestRepository
    {
        public EmergencyRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<EmergencyRequest> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .FirstOrDefaultAsync(er => er.Id == id);
        }

        public async Task<EmergencyRequest> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType)
                .FirstOrDefaultAsync(er => er.Id == id);
        }

        public async Task<(IEnumerable<EmergencyRequest> items, int totalCount)> GetPagedEmergencyRequestsAsync(EmergencyRequestParameters parameters)
        {
            IQueryable<EmergencyRequest> query = _dbSet
                .Include(er => er.BloodGroup)
                .Include(er => er.ComponentType);

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(er => er.Status.ToLower() == parameters.Status.ToLower());
            }

            if (!string.IsNullOrEmpty(parameters.UrgencyLevel))
            {
                query = query.Where(er => er.UrgencyLevel.ToLower() == parameters.UrgencyLevel.ToLower());
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(er => er.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(er => er.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(er => er.RequestDate >= parameters.StartDate);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(er => er.RequestDate <= parameters.EndDate);
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

        private IQueryable<EmergencyRequest> ApplySorting(IQueryable<EmergencyRequest> query, EmergencyRequestParameters parameters)
        {
            // Default sort is by urgency and date, newest first
            if (string.IsNullOrEmpty(parameters.SortBy))
            {
                return query.OrderByDescending(er => er.UrgencyLevel)
                            .ThenByDescending(er => er.RequestDate);
            }

            // Apply sorting based on the provided field
            return parameters.SortBy.ToLower() switch
            {
                "date" => parameters.SortAscending
                    ? query.OrderBy(er => er.RequestDate)
                    : query.OrderByDescending(er => er.RequestDate),
                
                "urgency" => parameters.SortAscending
                    ? query.OrderBy(er => er.UrgencyLevel)
                    : query.OrderByDescending(er => er.UrgencyLevel),
                
                "status" => parameters.SortAscending
                    ? query.OrderBy(er => er.Status)
                    : query.OrderByDescending(er => er.Status),
                
                "quantity" => parameters.SortAscending
                    ? query.OrderBy(er => er.QuantityUnits)
                    : query.OrderByDescending(er => er.QuantityUnits),
                
                "bloodgroup" => parameters.SortAscending
                    ? query.OrderBy(er => er.BloodGroup.GroupName)
                    : query.OrderByDescending(er => er.BloodGroup.GroupName),
                
                "componenttype" => parameters.SortAscending
                    ? query.OrderBy(er => er.ComponentType.Name)
                    : query.OrderByDescending(er => er.ComponentType.Name),
                
                "patient" => parameters.SortAscending
                    ? query.OrderBy(er => er.PatientName)
                    : query.OrderByDescending(er => er.PatientName),
                
                _ => query.OrderByDescending(er => er.UrgencyLevel)
                        .ThenByDescending(er => er.RequestDate),
            };
        }
    }
}