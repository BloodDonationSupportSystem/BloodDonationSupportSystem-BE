using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class ComponentType
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int ShelfLifeDays { get; set; }

        public ICollection<BloodInventory> BloodInventories { get; set; }
        public ICollection<DonationEvent> DonationEvents { get; set; }
    }
}
