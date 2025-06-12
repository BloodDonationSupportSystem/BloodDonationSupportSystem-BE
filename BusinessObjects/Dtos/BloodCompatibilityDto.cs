using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class BloodCompatibilityDto
    {
        public Guid RecipientBloodGroupId { get; set; }
        public string RecipientBloodGroupName { get; set; }
        public Guid DonorBloodGroupId { get; set; }
        public string DonorBloodGroupName { get; set; }
        public bool IsWholeBloodCompatible { get; set; }
        public bool IsRedCellsCompatible { get; set; }
        public bool IsPlasmaCompatible { get; set; }
        public bool IsPlateletCompatible { get; set; }
    }

    public class BloodCompatibilityLookupDto
    {
        public Guid BloodGroupId { get; set; }
        public string BloodGroupName { get; set; }
        public List<BloodGroupDto> CompatibleDonorGroups { get; set; } = new List<BloodGroupDto>();
        public List<BloodGroupDto> CompatibleRecipientGroups { get; set; } = new List<BloodGroupDto>();
    }
}