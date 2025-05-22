namespace BusinessObjects.Models
{
    public class Notification : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public Guid UserId { get; set; } = Guid.Empty;
        public virtual User User { get; set; } = new User();
    }
}
