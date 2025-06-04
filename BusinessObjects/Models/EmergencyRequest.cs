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
        public Guid BloodGroupId { get; set; } = Guid.Empty;
        public virtual BloodGroup BloodGroup { get; set; }
        public Guid ComponentTypeId { get; set; } = Guid.Empty;
        public virtual ComponentType ComponentType { get; set; }
    }
}
