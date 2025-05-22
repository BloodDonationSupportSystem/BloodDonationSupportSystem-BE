using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class RequestMatch : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset MatchDate { get; set; } = DateTimeOffset.UtcNow;
        public int UnitsAssigned { get; set; }
        public Guid RequestId { get; set; } = Guid.Empty;
        public virtual BloodRequest BloodRequest { get; set; } = new BloodRequest();
        public Guid EmergencyRequestId { get; set; } = Guid.Empty;
        public virtual EmergencyRequest EmergencyRequest { get; set; } = new EmergencyRequest();
        public Guid DonationEventId { get; set; } = Guid.Empty;
        public virtual DonationEvent DonationEvent { get; set; } = new DonationEvent();
    }
}
