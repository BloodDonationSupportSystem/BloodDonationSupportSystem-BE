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
    public class BloodDonationWorkflowRepository : GenericRepository<BloodDonationWorkflow>, IBloodDonationWorkflowRepository
    {
        public BloodDonationWorkflowRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BloodDonationWorkflow> GetByIdWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(w => w.BloodGroup)
                .Include(w => w.ComponentType)
                .Include(w => w.Donor)
                .Include(w => w.Inventory)
                .FirstOrDefaultAsync(w => w.Id == id && w.DeletedTime == null);
        }

        public async Task<IEnumerable<BloodDonationWorkflow>> GetByRequestAsync(Guid requestId, string requestType)
        {
            return await _dbSet
                .Include(w => w.BloodGroup)
                .Include(w => w.ComponentType)
                .Include(w => w.Donor)
                .Include(w => w.Inventory)
                .Where(w => w.RequestId == requestId && w.RequestType == requestType && w.DeletedTime == null)
                .OrderByDescending(w => w.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BloodDonationWorkflow>> GetByDonorIdAsync(Guid donorId)
        {
            return await _dbSet
                .Include(w => w.BloodGroup)
                .Include(w => w.ComponentType)
                .Include(w => w.Inventory)
                .Where(w => w.DonorId == donorId && w.DeletedTime == null)
                .OrderByDescending(w => w.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BloodDonationWorkflow>> GetByStatusAsync(string status)
        {
            return await _dbSet
                .Include(w => w.BloodGroup)
                .Include(w => w.ComponentType)
                .Include(w => w.Donor)
                .Include(w => w.Inventory)
                .Where(w => w.Status == status && w.DeletedTime == null && w.IsActive)
                .OrderByDescending(w => w.CreatedTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<BloodDonationWorkflow>> GetPendingAppointmentsAsync(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            var query = _dbSet
                .Include(w => w.BloodGroup)
                .Include(w => w.ComponentType)
                .Include(w => w.Donor)
                .Where(w => w.Status == "Scheduled" && !w.AppointmentConfirmed && w.DonorId != null && w.AppointmentDate != null && w.DeletedTime == null);

            if (startDate.HasValue)
            {
                query = query.Where(w => w.AppointmentDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(w => w.AppointmentDate <= endDate.Value);
            }

            return await query.OrderBy(w => w.AppointmentDate).ToListAsync();
        }

        public async Task<(IEnumerable<BloodDonationWorkflow>, int)> GetPagedWorkflowsAsync(DonationWorkflowParameters parameters)
        {
            var query = _dbSet
                .Include(w => w.BloodGroup)
                .Include(w => w.ComponentType)
                .Include(w => w.Donor)
                .Include(w => w.Inventory)
                .Where(w => w.DeletedTime == null)
                .AsQueryable();

            // Apply filters
            if (parameters.RequestId.HasValue)
            {
                query = query.Where(w => w.RequestId == parameters.RequestId.Value);
            }

            if (!string.IsNullOrEmpty(parameters.RequestType))
            {
                query = query.Where(w => w.RequestType == parameters.RequestType);
            }

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(w => w.Status == parameters.Status);
            }

            if (parameters.DonorId.HasValue)
            {
                query = query.Where(w => w.DonorId == parameters.DonorId);
            }

            if (parameters.BloodGroupId.HasValue)
            {
                query = query.Where(w => w.BloodGroupId == parameters.BloodGroupId);
            }

            if (parameters.ComponentTypeId.HasValue)
            {
                query = query.Where(w => w.ComponentTypeId == parameters.ComponentTypeId);
            }

            if (parameters.StartDate.HasValue)
            {
                query = query.Where(w => w.CreatedTime >= parameters.StartDate.Value);
            }

            if (parameters.EndDate.HasValue)
            {
                query = query.Where(w => w.CreatedTime <= parameters.EndDate.Value);
            }

            if (parameters.IsActive.HasValue)
            {
                query = query.Where(w => w.IsActive == parameters.IsActive.Value);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = ApplySorting(query, parameters.SortBy, parameters.SortAscending);
            }
            else
            {
                // Default sorting by creation date
                query = query.OrderByDescending(w => w.CreatedTime);
            }

            // Apply pagination
            var workflows = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return (workflows, totalCount);
        }

        public async Task<bool> CheckCompatibleBloodInventoryAsync(Guid bloodGroupId, Guid componentTypeId, double requiredQuantity)
        {
            // Check if there's available inventory for the blood group and component type
            var availableInventory = await _context.BloodInventories
                .Where(i => i.BloodGroupId == bloodGroupId && 
                       i.ComponentTypeId == componentTypeId && 
                       i.QuantityUnits >= requiredQuantity && 
                       i.ExpirationDate > DateTimeOffset.UtcNow &&
                       i.Status == "Available")
                .AnyAsync();

            return availableInventory;
        }

        public async Task<BloodInventory> GetCompatibleBloodInventoryAsync(Guid bloodGroupId, Guid componentTypeId, double requiredQuantity)
        {
            // Get the best match from inventory (oldest expiration date first to optimize inventory management)
            return await _context.BloodInventories
                .Where(i => i.BloodGroupId == bloodGroupId && 
                       i.ComponentTypeId == componentTypeId && 
                       i.QuantityUnits >= requiredQuantity && 
                       i.ExpirationDate > DateTimeOffset.UtcNow &&
                       i.Status == "Available")
                .OrderBy(i => i.ExpirationDate)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string newStatus, string statusDescription = null)
        {
            var workflow = await GetByIdAsync(id);
            if (workflow == null)
            {
                return false;
            }

            workflow.Status = newStatus;
            workflow.StatusDescription = statusDescription;
            workflow.LastUpdatedTime = DateTimeOffset.UtcNow;

            // Set completion time if the status is "Completed"
            if (newStatus == "Completed")
            {
                workflow.CompletedTime = DateTimeOffset.UtcNow;
            }

            Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignDonorAsync(Guid id, Guid donorId, DateTimeOffset? appointmentDate = null, string appointmentLocation = null)
        {
            var workflow = await GetByIdAsync(id);
            if (workflow == null)
            {
                return false;
            }

            workflow.DonorId = donorId;
            workflow.Status = "DonorAssigned";
            
            if (appointmentDate.HasValue)
            {
                workflow.AppointmentDate = appointmentDate;
                workflow.AppointmentLocation = appointmentLocation;
                workflow.Status = "Scheduled";
            }
            
            workflow.LastUpdatedTime = DateTimeOffset.UtcNow;
            
            Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FulfillFromInventoryAsync(Guid id, int inventoryId)
        {
            var workflow = await GetByIdAsync(id);
            var inventory = await _context.BloodInventories.FindAsync(inventoryId);
            
            if (workflow == null || inventory == null || inventory.Status != "Available")
            {
                return false;
            }

            // Update the workflow with inventory information
            workflow.InventoryId = inventoryId;
            workflow.Status = "CompletedFromInventory";
            workflow.CompletedTime = DateTimeOffset.UtcNow;
            workflow.LastUpdatedTime = DateTimeOffset.UtcNow;
            
            // Update the inventory status
            inventory.Status = "Reserved";
            
            Update(workflow);
            _context.BloodInventories.Update(inventory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CompleteDonationAsync(Guid id, DateTimeOffset donationDate, string donationLocation, double quantityDonated)
        {
            var workflow = await GetByIdAsync(id);
            if (workflow == null)
            {
                return false;
            }

            workflow.Status = "Completed";
            workflow.DonationDate = donationDate;
            workflow.DonationLocation = donationLocation;
            workflow.QuantityDonated = quantityDonated;
            workflow.CompletedTime = DateTimeOffset.UtcNow;
            workflow.LastUpdatedTime = DateTimeOffset.UtcNow;
            
            Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmAppointmentAsync(Guid id)
        {
            var workflow = await GetByIdAsync(id);
            if (workflow == null || workflow.Status != "Scheduled" || workflow.AppointmentDate == null)
            {
                return false;
            }

            workflow.AppointmentConfirmed = true;
            workflow.LastUpdatedTime = DateTimeOffset.UtcNow;
            
            Update(workflow);
            await _context.SaveChangesAsync();
            return true;
        }

        private IQueryable<BloodDonationWorkflow> ApplySorting(IQueryable<BloodDonationWorkflow> query, string sortBy, bool sortAscending)
        {
            switch (sortBy.ToLower())
            {
                case "createdate":
                    return sortAscending ? query.OrderBy(w => w.CreatedTime) : query.OrderByDescending(w => w.CreatedTime);
                case "status":
                    return sortAscending ? query.OrderBy(w => w.Status) : query.OrderByDescending(w => w.Status);
                case "appointmentdate":
                    return sortAscending ? query.OrderBy(w => w.AppointmentDate) : query.OrderByDescending(w => w.AppointmentDate);
                case "donationdate":
                    return sortAscending ? query.OrderBy(w => w.DonationDate) : query.OrderByDescending(w => w.DonationDate);
                case "bloodgroup":
                    return sortAscending ? query.OrderBy(w => w.BloodGroup.GroupName) : query.OrderByDescending(w => w.BloodGroup.GroupName);
                case "donor":
                    // Using the donor's ID since we don't have direct access to FirstName
                    return sortAscending ? query.OrderBy(w => w.DonorId) : query.OrderByDescending(w => w.DonorId);
                default:
                    return sortAscending ? query.OrderBy(w => w.CreatedTime) : query.OrderByDescending(w => w.CreatedTime);
            }
        }
    }
}