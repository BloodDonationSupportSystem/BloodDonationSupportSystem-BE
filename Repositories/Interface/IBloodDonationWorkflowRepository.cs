using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IBloodDonationWorkflowRepository : IGenericRepository<BloodDonationWorkflow>
    {
        Task<BloodDonationWorkflow> GetByIdWithDetailsAsync(Guid id);
        
        Task<IEnumerable<BloodDonationWorkflow>> GetByRequestAsync(Guid requestId, string requestType);
        
        Task<IEnumerable<BloodDonationWorkflow>> GetByDonorIdAsync(Guid donorId);
        
        Task<IEnumerable<BloodDonationWorkflow>> GetByStatusAsync(string status);
        
        Task<IEnumerable<BloodDonationWorkflow>> GetPendingAppointmentsAsync(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
        
        Task<(IEnumerable<BloodDonationWorkflow>, int)> GetPagedWorkflowsAsync(DonationWorkflowParameters parameters);
        
        Task<bool> CheckCompatibleBloodInventoryAsync(Guid bloodGroupId, Guid componentTypeId, double requiredQuantity);
        
        Task<BloodInventory> GetCompatibleBloodInventoryAsync(Guid bloodGroupId, Guid componentTypeId, double requiredQuantity);
        
        Task<bool> UpdateStatusAsync(Guid id, string newStatus, string statusDescription = null);
        
        Task<bool> AssignDonorAsync(Guid id, Guid donorId, DateTimeOffset? appointmentDate = null, string appointmentLocation = null);
        
        Task<bool> FulfillFromInventoryAsync(Guid id, int inventoryId);
        
        Task<bool> CompleteDonationAsync(Guid id, DateTimeOffset donationDate, string donationLocation, double quantityDonated);
        
        Task<bool> ConfirmAppointmentAsync(Guid id);
    }
}