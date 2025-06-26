using System;
using System.Collections.Generic;

namespace BusinessObjects.Models
{
    public class Location : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }

        // Navigation properties
        public virtual ICollection<LocationCapacity> LocationCapacities { get; set; } = new List<LocationCapacity>();
        public virtual ICollection<LocationStaffAssignment> StaffAssignments { get; set; } = new List<LocationStaffAssignment>();
        public virtual ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
        public virtual ICollection<EmergencyRequest> EmergencyRequests { get; set; } = new List<EmergencyRequest>();
        public virtual ICollection<DonationEvent> DonationEvents { get; set; } = new List<DonationEvent>();
        public virtual ICollection<DonationAppointmentRequest> AppointmentRequests { get; set; } = new List<DonationAppointmentRequest>();
        public virtual ICollection<DonationAppointmentRequest> ConfirmedAppointmentRequests { get; set; } = new List<DonationAppointmentRequest>();
    }
}
