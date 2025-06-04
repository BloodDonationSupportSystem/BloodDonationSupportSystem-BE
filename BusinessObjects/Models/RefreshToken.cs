using System;

namespace BusinessObjects.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; }
        public DateTimeOffset ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
    }
}