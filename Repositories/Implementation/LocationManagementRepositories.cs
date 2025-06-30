using BusinessObjects.Data;
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
    public class LocationCapacityRepository : GenericRepository<LocationCapacity>, ILocationCapacityRepository
    {
        public LocationCapacityRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<LocationCapacity>> GetByLocationIdAsync(Guid locationId)
        {
            return await _dbSet
                .Include(lc => lc.Location)
                .Where(lc => lc.LocationId == locationId && lc.DeletedTime == null)
                .OrderBy(lc => lc.TimeSlot)
                .ThenBy(lc => lc.DayOfWeek)
                .ToListAsync();
        }

        public async Task<IEnumerable<LocationCapacity>> GetActiveCapacitiesAsync(Guid locationId, DateTimeOffset date)
        {
            return await _dbSet
                .Where(lc => lc.LocationId == locationId && 
                           lc.IsActive && 
                           lc.DeletedTime == null &&
                           (lc.EffectiveDate == null || lc.EffectiveDate <= date) &&
                           (lc.ExpiryDate == null || lc.ExpiryDate >= date))
                .ToListAsync();
        }

        public async Task<LocationCapacity?> GetCapacityForTimeSlotAsync(Guid locationId, string timeSlot, DateTimeOffset date)
        {
            var dayOfWeek = date.DayOfWeek;
            
            return await _dbSet
                .Where(lc => lc.LocationId == locationId && 
                           lc.TimeSlot == timeSlot &&
                           lc.IsActive && 
                           lc.DeletedTime == null &&
                           (lc.DayOfWeek == null || lc.DayOfWeek == dayOfWeek) &&
                           (lc.EffectiveDate == null || lc.EffectiveDate <= date) &&
                           (lc.ExpiryDate == null || lc.ExpiryDate >= date))
                .OrderByDescending(lc => lc.DayOfWeek.HasValue) // Prefer day-specific over general
                .ThenByDescending(lc => lc.EffectiveDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasConflictingCapacityAsync(Guid locationId, string timeSlot, DayOfWeek? dayOfWeek, DateTimeOffset? effectiveDate, DateTimeOffset? expiryDate, Guid? excludeId = null)
        {
            var query = _dbSet.Where(lc => lc.LocationId == locationId && 
                                         lc.TimeSlot == timeSlot &&
                                         lc.IsActive && 
                                         lc.DeletedTime == null &&
                                         (lc.DayOfWeek == dayOfWeek || lc.DayOfWeek == null || dayOfWeek == null));

            if (excludeId.HasValue)
            {
                query = query.Where(lc => lc.Id != excludeId.Value);
            }

            // Check for date range overlaps
            if (effectiveDate.HasValue || expiryDate.HasValue)
            {
                query = query.Where(lc => 
                    (lc.EffectiveDate == null && lc.ExpiryDate == null) || // No date restrictions
                    (effectiveDate == null && expiryDate == null) || // New capacity has no date restrictions
                    (lc.ExpiryDate == null || expiryDate == null || lc.ExpiryDate >= effectiveDate) &&
                    (lc.EffectiveDate == null || effectiveDate == null || lc.EffectiveDate <= expiryDate));
            }

            return await query.AnyAsync();
        }
    }

    public class LocationStaffAssignmentRepository : GenericRepository<LocationStaffAssignment>, ILocationStaffAssignmentRepository
    {
        public LocationStaffAssignmentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<LocationStaffAssignment>> GetByLocationIdAsync(Guid locationId)
        {
            return await _dbSet
                .Include(lsa => lsa.Location)
                .Include(lsa => lsa.User)
                .Where(lsa => lsa.LocationId == locationId && lsa.DeletedTime == null)
                .OrderByDescending(lsa => lsa.IsActive)
                .ThenBy(lsa => lsa.Role)
                .ThenBy(lsa => lsa.User.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<LocationStaffAssignment>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(lsa => lsa.Location)
                .Include(lsa => lsa.User)
                .Where(lsa => lsa.UserId == userId && lsa.DeletedTime == null)
                .OrderByDescending(lsa => lsa.IsActive)
                .ThenBy(lsa => lsa.Location.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<LocationStaffAssignment>> GetActiveAssignmentsAsync(Guid locationId)
        {
            return await _dbSet
                .Include(lsa => lsa.User)
                .Where(lsa => lsa.LocationId == locationId && 
                           lsa.IsActive && 
                           lsa.DeletedTime == null &&
                           (lsa.UnassignedDate == null || lsa.UnassignedDate > DateTimeOffset.UtcNow))
                .ToListAsync();
        }

        public async Task<LocationStaffAssignment?> GetActiveAssignmentAsync(Guid locationId, Guid userId)
        {
            return await _dbSet
                .Include(lsa => lsa.Location)
                .Include(lsa => lsa.User)
                .FirstOrDefaultAsync(lsa => lsa.LocationId == locationId && 
                                          lsa.UserId == userId && 
                                          lsa.IsActive && 
                                          lsa.DeletedTime == null &&
                                          (lsa.UnassignedDate == null || lsa.UnassignedDate > DateTimeOffset.UtcNow));
        }

        public async Task<bool> IsUserAssignedToLocationAsync(Guid userId, Guid locationId)
        {
            return await _dbSet
                .AnyAsync(lsa => lsa.LocationId == locationId && 
                               lsa.UserId == userId && 
                               lsa.IsActive && 
                               lsa.DeletedTime == null &&
                               (lsa.UnassignedDate == null || lsa.UnassignedDate > DateTimeOffset.UtcNow));
        }

        public async Task<bool> CanUserManageLocationAsync(Guid userId, Guid locationId)
        {
            return await _dbSet
                .AnyAsync(lsa => lsa.LocationId == locationId && 
                               lsa.UserId == userId && 
                               lsa.IsActive && 
                               lsa.DeletedTime == null &&
                               lsa.CanManageCapacity &&
                               (lsa.UnassignedDate == null || lsa.UnassignedDate > DateTimeOffset.UtcNow));
        }

        public async Task<bool> CanUserApproveAppointmentsAsync(Guid userId, Guid locationId)
        {
            return await _dbSet
                .AnyAsync(lsa => lsa.LocationId == locationId && 
                               lsa.UserId == userId && 
                               lsa.IsActive && 
                               lsa.DeletedTime == null &&
                               lsa.CanApproveAppointments &&
                               (lsa.UnassignedDate == null || lsa.UnassignedDate > DateTimeOffset.UtcNow));
        }
    }
}