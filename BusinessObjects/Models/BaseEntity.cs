using System;

namespace BusinessObjects.Models
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? CreatedBy { get; set; }

        public string? LastUpdatedBy { get; set; }

        public DateTimeOffset? CreatedTime { get; set; }

        public DateTimeOffset? LastUpdatedTime { get; set; }

        public DateTimeOffset? DeletedTime { get; set; }
    }
}
