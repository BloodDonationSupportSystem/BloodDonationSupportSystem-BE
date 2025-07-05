using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects.Models
{
    public class DonationEvent : BaseEntity
    {
        [Key]
        public new Guid Id { get; set; } = Guid.NewGuid();
        
        // Thông tin người hiến máu
        public Guid? DonorId { get; set; }
        [ForeignKey("DonorId")]
        public virtual DonorProfile DonorProfile { get; set; }
        
        // Thông tin yêu cầu máu (nếu có)
        public Guid? RequestId { get; set; }
        [StringLength(50)]
        public string RequestType { get; set; } = string.Empty; // BloodRequest, EmergencyRequest, Appointment hoặc DirectDonation
        
        // Thông tin loại máu
        public Guid BloodGroupId { get; set; }
        [ForeignKey("BloodGroupId")]
        public virtual BloodGroup BloodGroup { get; set; }
        
        public Guid ComponentTypeId { get; set; }
        [ForeignKey("ComponentTypeId")]
        public virtual ComponentType ComponentType { get; set; }
        
        // Thông tin vị trí và nhân viên
        public Guid LocationId { get; set; }
        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
        
        public Guid? StaffId { get; set; }
        [ForeignKey("StaffId")]
        public virtual User Staff { get; set; }
        
        // Loại bỏ inventory relationship vì DonationEvent không còn quản lý inventory trực tiếp
        // [ForeignKey("InventoryId")]
        // public virtual BloodInventory Inventory { get; set; }
        
        // Thông tin trạng thái
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Created"; // Created, DonorAssigned, Scheduled, CheckedIn, HealthCheckPassed, HealthCheckFailed, InProgress, Incomplete, Completed, Cancelled
        
        [StringLength(255)]
        public string StatusDescription { get; set; } = string.Empty;
        
        // Thông tin lịch hẹn
        public DateTimeOffset? AppointmentDate { get; set; }
        [StringLength(255)]
        public string AppointmentLocation { get; set; } = string.Empty;
        public bool AppointmentConfirmed { get; set; } = false;
        
        // Thông tin check-in và kiểm tra sức khỏe
        public DateTimeOffset? CheckInTime { get; set; }
        public string BloodPressure { get; set; } = string.Empty;
        public double? Temperature { get; set; }
        public double? HemoglobinLevel { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }
        public string MedicalNotes { get; set; } = string.Empty;
        public string RejectionReason { get; set; } = string.Empty;
        
        // Thông tin quá trình hiến máu
        public DateTimeOffset? DonationStartTime { get; set; }
        public string ComplicationType { get; set; } = string.Empty;
        public string ComplicationDetails { get; set; } = string.Empty;
        public string ActionTaken { get; set; } = string.Empty;
        public bool IsUsable { get; set; } = true;
        
        // Thông tin hiến máu
        public DateTimeOffset? DonationDate { get; set; }
        [StringLength(255)]
        public string DonationLocation { get; set; } = string.Empty;
        public double? QuantityDonated { get; set; }
        
        // Thông tin thu thập (legacy fields - giữ để tương thích)
        public int QuantityUnits { get; set; } = 0;
        [StringLength(255)]
        public string CollectedAt { get; set; } = string.Empty;
        
        // Ghi chú bổ sung
        [StringLength(1000)]
        public string Notes { get; set; } = string.Empty;
        
        // Thông tin theo dõi - override BaseEntity
        public new DateTimeOffset? LastUpdatedTime { get; set; }
        public DateTimeOffset? CompletedTime { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
}
