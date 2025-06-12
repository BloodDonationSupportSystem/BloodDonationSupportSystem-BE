using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class BloodDonationWorkflow
    {
        [Key]
        public Guid Id { get; set; }
        
        // Relations to blood request or emergency request
        public Guid RequestId { get; set; }
        
        // Type of request (BloodRequest or EmergencyRequest)
        [Required]
        [StringLength(50)]
        public string RequestType { get; set; }
        
        // Donor information (if assigned)
        public Guid? DonorId { get; set; }
        [ForeignKey("DonorId")]
        public DonorProfile Donor { get; set; }
        
        // Blood details
        public Guid BloodGroupId { get; set; }
        [ForeignKey("BloodGroupId")]
        public BloodGroup BloodGroup { get; set; }
        
        public Guid ComponentTypeId { get; set; }
        [ForeignKey("ComponentTypeId")]
        public ComponentType ComponentType { get; set; }
        
        // Inventory relation (if fulfilled from inventory)
        public int? InventoryId { get; set; }
        [ForeignKey("InventoryId")]
        public BloodInventory Inventory { get; set; }
        
        // Workflow status (e.g., Created, DonorAssigned, Scheduled, Completed, Cancelled)
        [Required]
        [StringLength(50)]
        public string Status { get; set; }
        
        // Additional status description
        [StringLength(255)]
        public string StatusDescription { get; set; }
        
        // Appointment details
        public DateTimeOffset? AppointmentDate { get; set; }
        
        [StringLength(255)]
        public string AppointmentLocation { get; set; }
        
        public bool AppointmentConfirmed { get; set; }
        
        // Donation details
        public DateTimeOffset? DonationDate { get; set; }
        
        [StringLength(255)]
        public string DonationLocation { get; set; }
        
        public double? QuantityDonated { get; set; }
        
        // Tracking dates
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
        public DateTimeOffset? DeletedTime { get; set; }
        
        // Additional information
        [StringLength(1000)]
        public string Notes { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}