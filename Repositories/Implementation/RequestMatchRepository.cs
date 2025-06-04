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
    public class RequestMatchRepository : GenericRepository<RequestMatch>, IRequestMatchRepository
    {
        public RequestMatchRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RequestMatch> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(rm => rm.BloodRequest)
                    .ThenInclude(br => br.BloodGroup)
                .Include(rm => rm.BloodRequest)
                    .ThenInclude(br => br.ComponentType)
                .Include(rm => rm.EmergencyRequest)
                    .ThenInclude(er => er.BloodGroup)
                .Include(rm => rm.EmergencyRequest)
                    .ThenInclude(er => er.ComponentType)
                .Include(rm => rm.DonationEvent)
                    .ThenInclude(de => de.BloodGroup)
                .Include(rm => rm.DonationEvent)
                    .ThenInclude(de => de.ComponentType)
                .Include(rm => rm.DonationEvent)
                    .ThenInclude(de => de.DonorProfile)
                .FirstOrDefaultAsync(rm => rm.Id == id && rm.DeletedTime == null);
        }

        public async Task<IEnumerable<RequestMatch>> GetByRequestIdAsync(Guid requestId)
        {
            return await _dbSet
                .Include(rm => rm.BloodRequest)
                .Include(rm => rm.EmergencyRequest)
                .Include(rm => rm.DonationEvent)
                .Where(rm => rm.RequestId == requestId && rm.DeletedTime == null)
                .OrderByDescending(rm => rm.MatchDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestMatch>> GetByEmergencyRequestIdAsync(Guid emergencyRequestId)
        {
            return await _dbSet
                .Include(rm => rm.BloodRequest)
                .Include(rm => rm.EmergencyRequest)
                .Include(rm => rm.DonationEvent)
                .Where(rm => rm.EmergencyRequestId == emergencyRequestId && rm.DeletedTime == null)
                .OrderByDescending(rm => rm.MatchDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<RequestMatch>> GetByDonationEventIdAsync(Guid donationEventId)
        {
            return await _dbSet
                .Include(rm => rm.BloodRequest)
                .Include(rm => rm.EmergencyRequest)
                .Include(rm => rm.DonationEvent)
                .Where(rm => rm.DonationEventId == donationEventId && rm.DeletedTime == null)
                .OrderByDescending(rm => rm.MatchDate)
                .ToListAsync();
        }

        public async Task<(IEnumerable<RequestMatch> requestMatches, int totalCount)> GetPagedRequestMatchesAsync(RequestMatchParameters parameters)
        {
            var query = _dbSet.AsQueryable();

            // Apply filters
            if (parameters.RequestId.HasValue)
            {
                query = query.Where(rm => rm.RequestId == parameters.RequestId.Value);
            }

            if (parameters.EmergencyRequestId.HasValue)
            {
                query = query.Where(rm => rm.EmergencyRequestId == parameters.EmergencyRequestId.Value);
            }

            if (parameters.DonationEventId.HasValue)
            {
                query = query.Where(rm => rm.DonationEventId == parameters.DonationEventId.Value);
            }

            if (parameters.MatchDateFrom.HasValue)
            {
                query = query.Where(rm => rm.MatchDate >= parameters.MatchDateFrom.Value);
            }

            if (parameters.MatchDateTo.HasValue)
            {
                query = query.Where(rm => rm.MatchDate <= parameters.MatchDateTo.Value);
            }

            // Filter soft deleted
            query = query.Where(rm => rm.DeletedTime == null);

            // Get total count
            var totalCount = await query.CountAsync();

            // Include related entities
            query = query
                .Include(rm => rm.BloodRequest)
                    .ThenInclude(br => br.BloodGroup)
                .Include(rm => rm.BloodRequest)
                    .ThenInclude(br => br.ComponentType)
                .Include(rm => rm.EmergencyRequest)
                    .ThenInclude(er => er.BloodGroup)
                .Include(rm => rm.EmergencyRequest)
                    .ThenInclude(er => er.ComponentType)
                .Include(rm => rm.DonationEvent)
                    .ThenInclude(de => de.BloodGroup)
                .Include(rm => rm.DonationEvent)
                    .ThenInclude(de => de.ComponentType);

            // Apply sorting
            query = parameters.SortBy?.ToLower() switch
            {
                "matchdate" => parameters.SortAscending 
                    ? query.OrderBy(rm => rm.MatchDate) 
                    : query.OrderByDescending(rm => rm.MatchDate),
                "unitsassigned" => parameters.SortAscending 
                    ? query.OrderBy(rm => rm.UnitsAssigned) 
                    : query.OrderByDescending(rm => rm.UnitsAssigned),
                _ => parameters.SortAscending 
                    ? query.OrderBy(rm => rm.CreatedTime) 
                    : query.OrderByDescending(rm => rm.CreatedTime)
            };

            // Apply pagination
            var requestMatches = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (requestMatches, totalCount);
        }
    }
}