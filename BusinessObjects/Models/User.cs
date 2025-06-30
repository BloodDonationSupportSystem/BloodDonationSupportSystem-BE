using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTimeOffset LastLogin { get; set; } = DateTimeOffset.UtcNow;
        public Guid RoleId { get; set; } = Guid.Empty;
        public virtual Role Role { get; set; }
        public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
        public bool IsActivated { get; set; } = true;

        // Navigation properties
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
