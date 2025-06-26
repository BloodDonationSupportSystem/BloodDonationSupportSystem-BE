using System;

namespace BusinessObjects.Models
{
    /// <summary>
    /// Qu?n lý capacity cho t?ng time slot c?a location
    /// </summary>
    public class LocationCapacity : BaseEntity
    {
        public Guid LocationId { get; set; }
        public string TimeSlot { get; set; } = string.Empty; // Morning, Afternoon, Evening
        public int TotalCapacity { get; set; } = 10; // T?ng s? ch?
        public DayOfWeek? DayOfWeek { get; set; } // null = áp d?ng cho t?t c? các ngày
        public DateTimeOffset? EffectiveDate { get; set; } // Ngày b?t ??u có hi?u l?c
        public DateTimeOffset? ExpiryDate { get; set; } // Ngày h?t hi?u l?c
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Location Location { get; set; } = null!;
    }
}