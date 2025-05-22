namespace BusinessObjects.Models
{
    public class BlogPost : BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = false;
        public Guid AuthorId { get; set; } = Guid.Empty;
        public virtual User User { get; set; } = new User();
    }
}
