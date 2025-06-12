using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.Models;

namespace BusinessObjects.Dtos
{
    // DTO for creating a new donation request workflow
    public class CreateDonationWorkflowDto
    {
        // Request Information
        [Required]
        public Guid RequestId { get; set; }
        
        // Request Type (BloodRequest or EmergencyRequest)
        [Required]
        public string RequestType { get; set; }
        
        // Optional fields for blood inventory check
        public bool CheckInventoryFirst { get; set; } = true;
        
        // Initial notes for the workflow
        public string Notes { get; set; }
    }
    
    // DTO for updating the donation workflow
    public class UpdateDonationWorkflowDto
    {
        // The current status of the donation workflow
        public string Status { get; set; }
        
        // Optional donor assignment
        public Guid? DonorId { get; set; }
        
        // Optional inventory assignment
        public int? InventoryId { get; set; }
        
        // Optional appointment details
        public DateTimeOffset? AppointmentDate { get; set; }
        public string AppointmentLocation { get; set; }
        
        // Additional notes
        public string Notes { get; set; }
    }
    
    // DTO for returning donation workflow details
    public class DonationWorkflowDto
    {
        // Identifiers
        public Guid Id { get; set; }
        public Guid RequestId { get; set; }
        public string RequestType { get; set; }
        
        // Donor information (if assigned)
        public Guid? DonorId { get; set; }
        public string DonorName { get; set; }
        
        // Blood information
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        public Guid ComponentTypeId { get; set; }
        public string ComponentTypeName { get; set; }
        
        // Inventory information (if fulfilled from inventory)
        public int? InventoryId { get; set; }
        
        // Workflow status
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        
        // Appointment information (if applicable)
        public DateTimeOffset? AppointmentDate { get; set; }
        public string AppointmentLocation { get; set; }
        public bool AppointmentConfirmed { get; set; }
        
        // Donation details (if completed)
        public DateTimeOffset? DonationDate { get; set; }
        public string DonationLocation { get; set; }
        public double? QuantityDonated { get; set; }
        
        // Tracking dates
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
        
        // Additional information
        public string Notes { get; set; }
        public bool IsActive { get; set; }
    }
    
    // DTO for workflow status transition
    public class WorkflowStatusUpdateDto
    {
        [Required]
        public Guid WorkflowId { get; set; }
        
        [Required]
        public string NewStatus { get; set; }
        
        public string Notes { get; set; }
    }
    
    // DTO for assigning a donor to a workflow
    public class AssignDonorDto
    {
        [Required]
        public Guid WorkflowId { get; set; }
        
        [Required]
        public Guid DonorId { get; set; }
        
        public DateTimeOffset? AppointmentDate { get; set; }
        
        public string AppointmentLocation { get; set; }
        
        public string Notes { get; set; }
    }
    
    // DTO for fulfilling from inventory
    public class FulfillFromInventoryDto
    {
        [Required]
        public Guid WorkflowId { get; set; }
        
        [Required]
        public int InventoryId { get; set; }
        
        public string Notes { get; set; }
    }
    
    // DTO for completing a donation
    public class CompleteDonationDto
    {
        [Required]
        public Guid WorkflowId { get; set; }
        
        [Required]
        public DateTimeOffset DonationDate { get; set; }
        
        [Required]
        public string DonationLocation { get; set; }
        
        [Required]
        public double QuantityDonated { get; set; }
        
        public string Notes { get; set; }
    }
    
    // Parameters for searching donation workflows
    public class DonationWorkflowParameters : PaginationParameters
    {
        public Guid? RequestId { get; set; }
        public string RequestType { get; set; }
        public string Status { get; set; }
        public Guid? DonorId { get; set; }
        public Guid? BloodGroupId { get; set; }
        public Guid? ComponentTypeId { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool? IsActive { get; set; }
        public new string SortBy { get; set; }
        public new bool SortAscending { get; set; } = false;
    }
}