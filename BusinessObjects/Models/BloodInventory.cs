using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class BloodInventory
    {
        public int Id { get; set; }
        public int QuantityUnits { get; set; }
        public DateTimeOffset ExpirationDate { get; set; } = DateTimeOffset.UtcNow;
        public string Status { get; set; } = string.Empty;
        public string InventorySource { get; set; } = string.Empty;
        public Guid BloodGroupId { get; set; } = Guid.Empty;
        public virtual BloodGroup BloodGroup { get; set; }
        public Guid ComponentTypeId { get; set; } = Guid.Empty;
        public virtual ComponentType ComponentType { get; set; }
        public Guid DonationEventId { get; set; } = Guid.Empty;
        public virtual DonationEvent DonationEvent { get; set; }
        // New reference to blood request
        public Guid? FulfilledRequestId { get; set; }
        public DateTimeOffset? FulfilledDate { get; set; }
        public string? FulfillmentNotes { get; set; }

        // Optional navigation property
        public virtual BloodRequest FulfilledRequest { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
