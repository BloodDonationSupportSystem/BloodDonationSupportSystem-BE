using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface ILocationCapacityRepository : IGenericRepository<LocationCapacity>
    {
        Task<IEnumerable<LocationCapacity>> GetByLocationIdAsync(Guid locationId);
        Task<IEnumerable<LocationCapacity>> GetActiveCapacitiesAsync(Guid locationId, DateTimeOffset date);
        Task<LocationCapacity?> GetCapacityForTimeSlotAsync(Guid locationId, string timeSlot, DateTimeOffset date);
        Task<bool> HasConflictingCapacityAsync(Guid locationId, string timeSlot, DayOfWeek? dayOfWeek, DateTimeOffset? effectiveDate, DateTimeOffset? expiryDate, Guid? excludeId = null);
    }

    public interface ILocationStaffAssignmentRepository : IGenericRepository<LocationStaffAssignment>
    {
        Task<IEnumerable<LocationStaffAssignment>> GetByLocationIdAsync(Guid locationId);
        Task<IEnumerable<LocationStaffAssignment>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<LocationStaffAssignment>> GetActiveAssignmentsAsync(Guid locationId);
        Task<LocationStaffAssignment?> GetActiveAssignmentAsync(Guid locationId, Guid userId);
        Task<bool> IsUserAssignedToLocationAsync(Guid userId, Guid locationId);
        Task<bool> CanUserManageLocationAsync(Guid userId, Guid locationId);
        Task<bool> CanUserApproveAppointmentsAsync(Guid userId, Guid locationId);
    }

    public interface ILocationOperatingHoursRepository : IGenericRepository<LocationOperatingHours>
    {
        Task<IEnumerable<LocationOperatingHours>> GetByLocationIdAsync(Guid locationId);
        Task<LocationOperatingHours?> GetByLocationAndDayAsync(Guid locationId, DayOfWeek dayOfWeek);
        Task<bool> IsLocationOpenAsync(Guid locationId, DayOfWeek dayOfWeek, string timeSlot);
        Task<TimeSpan?> GetTimeSlotHoursAsync(Guid locationId, DayOfWeek dayOfWeek, string timeSlot);
    }
}