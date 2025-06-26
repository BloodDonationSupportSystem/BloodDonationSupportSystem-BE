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
    public class DonationAppointmentRequestRepository : GenericRepository<DonationAppointmentRequest>, IDonationAppointmentRequestRepository
    {
        public DonationAppointmentRequestRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DonationAppointmentRequest?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(r => r.Donor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Donor)
                    .ThenInclude(d => d.BloodGroup)
                .Include(r => r.Location)
                .Include(r => r.BloodGroup)
                .Include(r => r.ComponentType)
                .Include(r => r.InitiatedByUser)
                .Include(r => r.ReviewedByUser)
                .Include(r => r.ConfirmedLocation)
                .Include(r => r.Workflow)
                .FirstOrDefaultAsync(r => r.Id == id && r.DeletedTime == null);
        }

        public async Task<(IEnumerable<DonationAppointmentRequest> items, int totalCount)> GetPagedAppointmentRequestsAsync(AppointmentRequestParameters parameters)
        {
            IQueryable<DonationAppointmentRequest> query = _dbSet
                .Include(r => r.Donor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Location)
                .Include(r => r.BloodGroup)
                .Include(r => r.ComponentType)
                .Include(r => r.InitiatedByUser)
                .Include(r => r.ReviewedByUser)
                .Include(r => r.ConfirmedLocation)
                .Where(r => r.DeletedTime == null);

            // Apply filters
            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(r => r.Status.ToLower() == parameters.Status.ToLower());
            }

            if (!string.IsNullOrEmpty(parameters.RequestType))
            {
                query = query.Where(r => r.RequestType.ToLower() == parameters.RequestType.ToLower());
            }

            if (parameters.DonorId.HasValue)
            {
                query = query.Where(r => r.DonorId == parameters.DonorId);
            }

            if (parameters.LocationId.HasValue)
            {
                query = query.Where(r => r.LocationId == parameters.LocationId || r.ConfirmedLocationId == parameters.LocationId);
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(r => r.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(r => r.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.InitiatedByUserId.HasValue)
            {
                query = query.Where(r => r.InitiatedByUserId == parameters.InitiatedByUserId);
            }

            if (parameters.ReviewedByUserId.HasValue)
            {
                query = query.Where(r => r.ReviewedByUserId == parameters.ReviewedByUserId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(r => r.PreferredDate >= parameters.StartDate);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(r => r.PreferredDate <= parameters.EndDate);
            }

            if (parameters.IsUrgent.HasValue)
            {
                query = query.Where(r => r.IsUrgent == parameters.IsUrgent);
            }

            if (parameters.Priority.HasValue)
            {
                query = query.Where(r => r.Priority == parameters.Priority);
            }

            if (parameters.RequiresDonorResponse.HasValue)
            {
                if (parameters.RequiresDonorResponse.Value)
                {
                    query = query.Where(r => r.RequestType == "StaffInitiated" && r.Status == "Pending" && r.DonorAccepted == null);
                }
                else
                {
                    query = query.Where(r => !(r.RequestType == "StaffInitiated" && r.Status == "Pending" && r.DonorAccepted == null));
                }
            }

            if (parameters.IsExpired.HasValue)
            {
                var now = DateTimeOffset.UtcNow;
                if (parameters.IsExpired.Value)
                {
                    query = query.Where(r => r.ExpiresAt.HasValue && r.ExpiresAt < now);
                }
                else
                {
                    query = query.Where(r => !r.ExpiresAt.HasValue || r.ExpiresAt >= now);
                }
            }

            // Apply sorting
            query = ApplySorting(query, parameters);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<IEnumerable<DonationAppointmentRequest>> GetRequestsByDonorIdAsync(Guid donorId)
        {
            return await _dbSet
                .Include(r => r.Location)
                .Include(r => r.BloodGroup)
                .Include(r => r.ComponentType)
                .Include(r => r.ConfirmedLocation)
                .Where(r => r.DonorId == donorId && r.DeletedTime == null)
                .OrderByDescending(r => r.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<DonationAppointmentRequest>> GetRequestsByStatusAsync(string status)
        {
            return await _dbSet
                .Include(r => r.Donor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Location)
                .Include(r => r.BloodGroup)
                .Include(r => r.ComponentType)
                .Where(r => r.Status.ToLower() == status.ToLower() && r.DeletedTime == null)
                .OrderByDescending(r => r.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<DonationAppointmentRequest>> GetPendingDonorResponsesAsync()
        {
            return await _dbSet
                .Include(r => r.Donor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Location)
                .Include(r => r.ConfirmedLocation)
                .Where(r => r.RequestType == "StaffInitiated" && 
                           r.Status == "Pending" && 
                           r.DonorAccepted == null && 
                           r.DeletedTime == null &&
                           (!r.ExpiresAt.HasValue || r.ExpiresAt > DateTimeOffset.UtcNow))
                .OrderBy(r => r.ExpiresAt ?? DateTimeOffset.MaxValue)
                .ToListAsync();
        }

        public async Task<IEnumerable<DonationAppointmentRequest>> GetPendingStaffReviewsAsync()
        {
            return await _dbSet
                .Include(r => r.Donor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Location)
                .Include(r => r.BloodGroup)
                .Include(r => r.ComponentType)
                .Where(r => r.RequestType == "DonorInitiated" && 
                           r.Status == "Pending" && 
                           r.ReviewedAt == null && 
                           r.DeletedTime == null)
                .OrderByDescending(r => r.IsUrgent)
                .ThenByDescending(r => r.Priority)
                .ThenBy(r => r.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<DonationAppointmentRequest>> GetExpiredRequestsAsync()
        {
            var now = DateTimeOffset.UtcNow;
            return await _dbSet
                .Include(r => r.Donor)
                    .ThenInclude(d => d.User)
                .Where(r => r.ExpiresAt.HasValue && 
                           r.ExpiresAt < now && 
                           r.Status == "Pending" && 
                           r.DeletedTime == null)
                .ToListAsync();
        }

        public async Task<IEnumerable<DonationAppointmentRequest>> GetUrgentRequestsAsync()
        {
            return await _dbSet
                .Include(r => r.Donor)
                    .ThenInclude(d => d.User)
                .Include(r => r.Location)
                .Include(r => r.BloodGroup)
                .Include(r => r.ComponentType)
                .Where(r => r.IsUrgent && 
                           r.Status == "Pending" && 
                           r.DeletedTime == null)
                .OrderByDescending(r => r.Priority)
                .ThenBy(r => r.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<DonationAppointmentRequest>> GetRequestsByLocationAndDateAsync(Guid locationId, DateTimeOffset date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            return await _dbSet
                .Where(r => (r.LocationId == locationId || r.ConfirmedLocationId == locationId) &&
                           ((r.PreferredDate >= startOfDay && r.PreferredDate < endOfDay) ||
                            (r.ConfirmedDate.HasValue && r.ConfirmedDate >= startOfDay && r.ConfirmedDate < endOfDay)) &&
                           r.Status != "Cancelled" && 
                           r.Status != "Rejected" && 
                           r.DeletedTime == null)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetTimeSlotCapacityAsync(Guid locationId, DateTimeOffset date)
        {
            var requests = await GetRequestsByLocationAndDateAsync(locationId, date);
            
            var timeSlotCounts = new Dictionary<string, int>
            {
                { "Morning", 0 },
                { "Afternoon", 0 },
                { "Evening", 0 }
            };

            foreach (var request in requests)
            {
                var timeSlot = !string.IsNullOrEmpty(request.ConfirmedTimeSlot) 
                    ? request.ConfirmedTimeSlot 
                    : request.PreferredTimeSlot;

                if (timeSlotCounts.ContainsKey(timeSlot))
                {
                    timeSlotCounts[timeSlot]++;
                }
            }

            return timeSlotCounts;
        }

        public async Task<bool> UpdateStatusAsync(Guid requestId, string status, Guid? updatedByUserId = null)
        {
            var request = await GetByIdAsync(requestId);
            if (request == null) return false;

            request.Status = status;
            request.LastUpdatedTime = DateTimeOffset.UtcNow;
            
            if (updatedByUserId.HasValue)
            {
                request.ReviewedByUserId = updatedByUserId;
                request.ReviewedAt = DateTimeOffset.UtcNow;
            }

            Update(request);
            return true;
        }

        public async Task<bool> UpdateDonorResponseAsync(Guid requestId, bool accepted, string? notes = null)
        {
            var request = await GetByIdAsync(requestId);
            if (request == null) return false;

            request.DonorAccepted = accepted;
            request.DonorResponseAt = DateTimeOffset.UtcNow;
            request.DonorResponseNotes = notes;
            request.Status = accepted ? "Confirmed" : "Rejected";
            request.LastUpdatedTime = DateTimeOffset.UtcNow;

            Update(request);
            return true;
        }

        public async Task<bool> UpdateStaffResponseAsync(Guid requestId, DateTimeOffset? confirmedDate, string? confirmedTimeSlot, Guid? confirmedLocationId, string? notes = null)
        {
            var request = await GetByIdAsync(requestId);
            if (request == null) return false;

            request.ConfirmedDate = confirmedDate;
            request.ConfirmedTimeSlot = confirmedTimeSlot;
            request.ConfirmedLocationId = confirmedLocationId;
            request.Notes = notes;
            request.LastUpdatedTime = DateTimeOffset.UtcNow;

            Update(request);
            return true;
        }

        public async Task<bool> LinkToWorkflowAsync(Guid requestId, Guid workflowId)
        {
            var request = await GetByIdAsync(requestId);
            if (request == null) return false;

            request.WorkflowId = workflowId;
            request.LastUpdatedTime = DateTimeOffset.UtcNow;

            Update(request);
            return true;
        }

        public async Task<int> MarkExpiredRequestsAsync()
        {
            var expiredRequests = await GetExpiredRequestsAsync();
            int count = 0;

            foreach (var request in expiredRequests)
            {
                request.Status = "Expired";
                request.LastUpdatedTime = DateTimeOffset.UtcNow;
                Update(request);
                count++;
            }

            return count;
        }

        public async Task<IEnumerable<DonationAppointmentRequest>> GetRequestsExpiringInHoursAsync(int hours)
        {
            var expiryThreshold = DateTimeOffset.UtcNow.AddHours(hours);
            
            return await _dbSet
                .Include(r => r.Donor)
                    .ThenInclude(d => d.User)
                .Where(r => r.ExpiresAt.HasValue && 
                           r.ExpiresAt <= expiryThreshold && 
                           r.ExpiresAt > DateTimeOffset.UtcNow &&
                           r.Status == "Pending" && 
                           r.DeletedTime == null)
                .ToListAsync();
        }

        private IQueryable<DonationAppointmentRequest> ApplySorting(IQueryable<DonationAppointmentRequest> query, AppointmentRequestParameters parameters)
        {
            if (string.IsNullOrEmpty(parameters.SortBy))
            {
                return query.OrderByDescending(r => r.CreatedTime);
            }

            return parameters.SortBy.ToLower() switch
            {
                "date" => parameters.SortAscending 
                    ? query.OrderBy(r => r.PreferredDate) 
                    : query.OrderByDescending(r => r.PreferredDate),
                "status" => parameters.SortAscending 
                    ? query.OrderBy(r => r.Status) 
                    : query.OrderByDescending(r => r.Status),
                "priority" => parameters.SortAscending 
                    ? query.OrderBy(r => r.Priority) 
                    : query.OrderByDescending(r => r.Priority),
                "donorname" => parameters.SortAscending 
                    ? query.OrderBy(r => r.Donor.User.FirstName) 
                    : query.OrderByDescending(r => r.Donor.User.FirstName),
                "location" => parameters.SortAscending 
                    ? query.OrderBy(r => r.Location.Name) 
                    : query.OrderByDescending(r => r.Location.Name),
                _ => parameters.SortAscending 
                    ? query.OrderBy(r => r.CreatedTime) 
                    : query.OrderByDescending(r => r.CreatedTime)
            };
        }
    }
}