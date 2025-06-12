using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBloodDonationWorkflowService
    {
        // Get all donation workflows with paging
        Task<PagedApiResponse<DonationWorkflowDto>> GetPagedDonationWorkflowsAsync(DonationWorkflowParameters parameters);
        
        // Get a specific donation workflow by ID
        Task<ApiResponse<DonationWorkflowDto>> GetDonationWorkflowByIdAsync(Guid id);
        
        // Get workflows related to a specific blood request
        Task<ApiResponse<IEnumerable<DonationWorkflowDto>>> GetDonationWorkflowsByRequestAsync(Guid requestId, string requestType);
        
        // Get workflows assigned to a specific donor
        Task<ApiResponse<IEnumerable<DonationWorkflowDto>>> GetDonationWorkflowsByDonorAsync(Guid donorId);
        
        // Create a new donation workflow
        Task<ApiResponse<DonationWorkflowDto>> CreateDonationWorkflowAsync(CreateDonationWorkflowDto workflowDto);
        
        // Update an existing donation workflow
        Task<ApiResponse<DonationWorkflowDto>> UpdateDonationWorkflowAsync(Guid id, UpdateDonationWorkflowDto workflowDto);
        
        // Change the status of a donation workflow
        Task<ApiResponse<DonationWorkflowDto>> UpdateWorkflowStatusAsync(WorkflowStatusUpdateDto statusUpdateDto);
        
        // Assign a donor to a workflow
        Task<ApiResponse<DonationWorkflowDto>> AssignDonorAsync(AssignDonorDto assignDonorDto);
        
        // Fulfill a request from inventory
        Task<ApiResponse<DonationWorkflowDto>> FulfillFromInventoryAsync(FulfillFromInventoryDto fulfillDto);
        
        // Complete a donation
        Task<ApiResponse<DonationWorkflowDto>> CompleteDonationAsync(CompleteDonationDto completeDonationDto);
        
        // Cancel a donation workflow
        Task<ApiResponse<DonationWorkflowDto>> CancelDonationWorkflowAsync(Guid id, string reason);
        
        // Soft delete a donation workflow
        Task<ApiResponse> DeleteDonationWorkflowAsync(Guid id);
        
        // Get active workflows by status
        Task<ApiResponse<IEnumerable<DonationWorkflowDto>>> GetWorkflowsByStatusAsync(string status);
        
        // Get pending donation appointments
        Task<ApiResponse<IEnumerable<DonationWorkflowDto>>> GetPendingAppointmentsAsync(DateTimeOffset? startDate = null, DateTimeOffset? endDate = null);
        
        // Confirm a donation appointment
        Task<ApiResponse<DonationWorkflowDto>> ConfirmAppointmentAsync(Guid workflowId);
    }
}