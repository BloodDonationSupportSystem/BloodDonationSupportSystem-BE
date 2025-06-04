using System;
using Shared.Models;

namespace BusinessObjects.Dtos
{
    public class BlogPostDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsPublished { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public DateTimeOffset? LastUpdatedTime { get; set; }
    }

    public class CreateBlogPostDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsPublished { get; set; }
        public Guid AuthorId { get; set; }
    }

    public class UpdateBlogPostDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public bool IsPublished { get; set; }
    }
    
    public class BlogPostParameters : PaginationParameters
    {
        public Guid? AuthorId { get; set; }
        public bool? IsPublished { get; set; }
        public string SearchTerm { get; set; }
        public DateTimeOffset? CreatedDateFrom { get; set; }
        public DateTimeOffset? CreatedDateTo { get; set; }
        public DateTimeOffset? UpdatedDateFrom { get; set; }
        public DateTimeOffset? UpdatedDateTo { get; set; }
    }
}