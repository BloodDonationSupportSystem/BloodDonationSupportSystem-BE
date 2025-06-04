using Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

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
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Body is required")]
        public string Body { get; set; }

        public bool IsPublished { get; set; }

        [Required(ErrorMessage = "Author ID is required")]
        public Guid AuthorId { get; set; }
    }

    public class UpdateBlogPostDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Body is required")]
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