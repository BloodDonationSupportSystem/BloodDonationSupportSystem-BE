using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IDocumentService
    {
        Task<ApiResponse<IEnumerable<DocumentDto>>> GetAllDocumentsAsync();
        Task<ApiResponse<DocumentDto>> GetDocumentByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<DocumentDto>>> GetDocumentsByUserIdAsync(Guid userId);
        Task<ApiResponse<IEnumerable<DocumentDto>>> GetDocumentsByTypeAsync(string documentType);
        Task<ApiResponse<DocumentDto>> CreateDocumentAsync(CreateDocumentDto documentDto);
        Task<ApiResponse<DocumentDto>> UpdateDocumentAsync(Guid id, UpdateDocumentDto documentDto);
        Task<ApiResponse> DeleteDocumentAsync(Guid id);
        Task<PagedApiResponse<DocumentDto>> GetPagedDocumentsAsync(DocumentParameters parameters);
    }
}