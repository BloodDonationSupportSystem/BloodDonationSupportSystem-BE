using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IDonationAppointmentRequestRepository : IGenericRepository<DonationAppointmentRequest>
    {
        // Basic CRUD with details
        Task<DonationAppointmentRequest?> GetByIdWithDetailsAsync(Guid id);
        Task<(IEnumerable<DonationAppointmentRequest> items, int totalCount)> GetPagedAppointmentRequestsAsync(AppointmentRequestParameters parameters);
        
        // Get requests by different criteria
        Task<IEnumerable<DonationAppointmentRequest>> GetRequestsByDonorIdAsync(Guid donorId);
        Task<IEnumerable<DonationAppointmentRequest>> GetRequestsByStatusAsync(string status);
        Task<IEnumerable<DonationAppointmentRequest>> GetPendingDonorResponsesAsync();
        Task<IEnumerable<DonationAppointmentRequest>> GetPendingStaffReviewsAsync();
        Task<IEnumerable<DonationAppointmentRequest>> GetExpiredRequestsAsync();
        Task<IEnumerable<DonationAppointmentRequest>> GetUrgentRequestsAsync();
        
        // Location and time slot availability
        Task<IEnumerable<DonationAppointmentRequest>> GetRequestsByLocationAndDateAsync(Guid locationId, DateTimeOffset date);
        Task<Dictionary<string, int>> GetTimeSlotCapacityAsync(Guid locationId, DateTimeOffset date);
        
        // Update specific fields
        Task<bool> UpdateStatusAsync(Guid requestId, string status, Guid? updatedByUserId = null);
        Task<bool> UpdateDonorResponseAsync(Guid requestId, bool accepted, string? notes = null);
        Task<bool> UpdateStaffResponseAsync(Guid requestId, DateTimeOffset? confirmedDate, string? confirmedTimeSlot, Guid? confirmedLocationId, string? notes = null);
        Task<bool> LinkToWorkflowAsync(Guid requestId, Guid workflowId);
        
        // Cleanup and maintenance
        Task<int> MarkExpiredRequestsAsync();
        Task<IEnumerable<DonationAppointmentRequest>> GetRequestsExpiringInHoursAsync(int hours);
    }
}