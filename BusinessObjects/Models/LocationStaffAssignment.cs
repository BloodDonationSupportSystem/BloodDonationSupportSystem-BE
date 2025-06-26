using System;

namespace BusinessObjects.Models
{
    /// <summary>
    /// Qu?n lý assignment c?a staff cho locations
    /// </summary>
    public class LocationStaffAssignment : BaseEntity
    {
        public Guid LocationId { get; set; }
        public Guid UserId { get; set; } // Staff user
        public string Role { get; set; } = string.Empty; // LocationManager, Staff, Supervisor
        public bool CanManageCapacity { get; set; } = false; // Có th? qu?n lý capacity không
        public bool CanApproveAppointments { get; set; } = false; // Có th? approve appointments không
        public bool CanViewReports { get; set; } = false; // Có th? xem reports không
        public DateTimeOffset AssignedDate { get; set; }
        public DateTimeOffset? UnassignedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Location Location { get; set; } = null!;
        public virtual User User { get; set; } = null!;
    }
}