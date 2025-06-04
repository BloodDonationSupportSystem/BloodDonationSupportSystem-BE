using System;

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
        public string Title { get; set; }
        public string Content { get; set; }
        public string DocumentType { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class UpdateDocumentDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string DocumentType { get; set; }
    }
}