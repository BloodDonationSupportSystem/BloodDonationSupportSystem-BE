using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class BloodRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int QuantityUnits { get; set; }
        public DateTimeOffset RequestDate { get; set; } = DateTimeOffset.UtcNow;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset NeededByDate { get; set; } = DateTimeOffset.UtcNow;

        public Guid RequestedBy { get; set; }
        public virtual User User { get; set; }
        public Guid BloodGroupId { get; set; } = Guid.Empty;
        public virtual BloodGroup BloodGroup { get; set; }
        public Guid ComponentTypeId { get; set; } = Guid.Empty;
        public virtual ComponentType ComponentType { get; set; }
        public Guid LocationId { get; set; } = Guid.Empty;
        public virtual Location Location { get; set; }
    }
}
