using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class DonorProfile : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset DateOfBirth { get; set; } = DateTimeOffset.UtcNow;
        public bool Gender { get; set; }
        public DateTimeOffset LastDonationDate { get; set; } = DateTimeOffset.UtcNow;
        public string HealthStatus { get; set; } = string.Empty;
        public DateTimeOffset LastHealthCheckDate { get; set; } = DateTimeOffset.UtcNow;
        public int TotalDonations { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Latitude { get; set; } = string.Empty;
        public string Longitude { get; set; } = string.Empty;

        public Guid UserId { get; set; } = Guid.Empty;
        public virtual User User { get; set; } = new User();
        public Guid BloodGroupId { get; set; } = Guid.Empty;
        public virtual BloodGroup BloodGroup { get; set; } = new BloodGroup();
    }
}
