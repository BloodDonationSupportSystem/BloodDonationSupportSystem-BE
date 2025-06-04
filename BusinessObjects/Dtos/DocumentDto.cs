using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos
{
    public class DocumentDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string DocumentType { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public Guid CreatedBy { get; set; }
        public string CreatedByName { get; set; }
    }

    public class CreateDocumentDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Document type is required")]
        [StringLength(50, ErrorMessage = "Document type cannot be longer than 50 characters")]
        public string DocumentType { get; set; }

        [Required(ErrorMessage = "Created by user ID is required")]
        public Guid CreatedBy { get; set; }
    }

    public class UpdateDocumentDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot be longer than 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Document type is required")]
        [StringLength(50, ErrorMessage = "Document type cannot be longer than 50 characters")]
        public string DocumentType { get; set; }
    }
}