namespace BusinessObjects.Models
{
    public class BloodGroup
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
