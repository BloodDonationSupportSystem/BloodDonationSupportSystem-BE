using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class DonationEvent :BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int QuantityUnits { get; set; }
        public string CollectedAt { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public Guid DonorId { get; set; } = Guid.Empty;
        public virtual DonorProfile DonorProfile { get; set; } = new DonorProfile();
        public Guid BloodGroupId { get; set; } = Guid.Empty;
        public virtual BloodGroup BloodGroup { get; set; } = new BloodGroup();
        public Guid ComponentTypeId { get; set; } = Guid.Empty;
        public virtual ComponentType ComponentType { get; set; } = new ComponentType();
        public Guid LocationId { get; set; } = Guid.Empty;
        public virtual Location Location { get; set; } = new Location();
    }
}
