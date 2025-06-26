using System;

namespace BusinessObjects.Models
{
    /// <summary>
    /// Qu?n lý gi? ho?t ??ng c?a location
    /// </summary>
    public class LocationOperatingHours : BaseEntity
    {
        public Guid LocationId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan MorningStartTime { get; set; } = new TimeSpan(8, 0, 0); // 8:00 AM
        public TimeSpan MorningEndTime { get; set; } = new TimeSpan(12, 0, 0); // 12:00 PM
        public TimeSpan AfternoonStartTime { get; set; } = new TimeSpan(13, 0, 0); // 1:00 PM
        public TimeSpan AfternoonEndTime { get; set; } = new TimeSpan(17, 0, 0); // 5:00 PM
        public TimeSpan? EveningStartTime { get; set; } = new TimeSpan(18, 0, 0); // 6:00 PM
        public TimeSpan? EveningEndTime { get; set; } = new TimeSpan(21, 0, 0); // 9:00 PM
        public bool IsClosed { get; set; } = false; // ?óng c?a c? ngày
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // Navigation properties
        public virtual Location Location { get; set; } = null!;
    }
}