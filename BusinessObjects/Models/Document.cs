namespace BusinessObjects.Models
{
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

        public Guid CreatedBy { get; set; } = Guid.Empty;
        public virtual User User { get; set; }
    }
}
