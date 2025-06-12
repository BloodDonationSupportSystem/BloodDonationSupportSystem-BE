using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class EmergencyRequest : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PatientName { get; set; } = string.Empty;
        public int QuantityUnits { get; set; }
        public DateTimeOffset RequestDate { get; set; } = DateTimeOffset.UtcNow;
        public string UrgencyLevel { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Blood Group
        public Guid BloodGroupId { get; set; } = Guid.Empty;
        public virtual BloodGroup BloodGroup { get; set; }

        // Component Type
        public Guid ComponentTypeId { get; set; } = Guid.Empty;
        public virtual ComponentType ComponentType { get; set; }

        // Location information
        public Guid? LocationId { get; set; }
        public virtual Location Location { get; set; }

        // Additional location information if no LocationId is provided
        public string Address { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;

        // Hospital or medical facility information
        public string HospitalName { get; set; } = string.Empty;

        // Additional medical information
        public string MedicalNotes { get; set; } = string.Empty;

        // Is this request still active
        public bool IsActive { get; set; } = true;
    }
}
