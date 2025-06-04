using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class DocumentService : IDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DocumentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<DocumentDto>>> GetAllDocumentsAsync()
        {
            try
            {
                var documents = await _unitOfWork.Documents.GetAllAsync();
                var documentDtos = documents.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<DocumentDto>>(documentDtos)
                {
                    Message = $"Retrieved {documentDtos.Count} documents successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<DocumentDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DocumentDto>> GetDocumentByIdAsync(Guid id)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdWithUserAsync(id);
                
                if (document == null)
                    return new ApiResponse<DocumentDto>(HttpStatusCode.NotFound, $"Document with ID {id} not found");

                return new ApiResponse<DocumentDto>(MapToDto(document));
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<DocumentDto>>> GetDocumentsByUserIdAsync(Guid userId)
        {
            try
            {
                // Verify that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                {
                    return new ApiResponse<IEnumerable<DocumentDto>>(HttpStatusCode.BadRequest, $"User with ID {userId} not found");
                }

                var documents = await _unitOfWork.Documents.GetByUserIdAsync(userId);
                var documentDtos = documents.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<DocumentDto>>(documentDtos)
                {
                    Message = $"Retrieved {documentDtos.Count} documents for user {userId} successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<DocumentDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<DocumentDto>>> GetDocumentsByTypeAsync(string documentType)
        {
            try
            {
                var documents = await _unitOfWork.Documents.GetByDocumentTypeAsync(documentType);
                var documentDtos = documents.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<DocumentDto>>(documentDtos)
                {
                    Message = $"Retrieved {documentDtos.Count} documents of type {documentType} successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<DocumentDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DocumentDto>> CreateDocumentAsync(CreateDocumentDto documentDto)
        {
            try
            {
                // Verify that the user exists
                var user = await _unitOfWork.Users.GetByIdAsync(documentDto.CreatedBy);
                if (user == null)
                {
                    return new ApiResponse<DocumentDto>(HttpStatusCode.BadRequest, $"User with ID {documentDto.CreatedBy} not found");
                }

                var document = new Document
                {
                    Title = documentDto.Title,
                    Content = documentDto.Content,
                    DocumentType = documentDto.DocumentType,
                    CreatedBy = documentDto.CreatedBy,
                    CreatedDate = DateTimeOffset.UtcNow
                };

                await _unitOfWork.Documents.AddAsync(document);
                await _unitOfWork.CompleteAsync();

                // Fetch the document with user details
                var createdDocument = await _unitOfWork.Documents.GetByIdWithUserAsync(document.Id);

                return new ApiResponse<DocumentDto>(MapToDto(createdDocument), "Document created successfully")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<DocumentDto>> UpdateDocumentAsync(Guid id, UpdateDocumentDto documentDto)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdAsync(id);
                
                if (document == null)
                    return new ApiResponse<DocumentDto>(HttpStatusCode.NotFound, $"Document with ID {id} not found");

                document.Title = documentDto.Title;
                document.Content = documentDto.Content;
                document.DocumentType = documentDto.DocumentType;

                _unitOfWork.Documents.Update(document);
                await _unitOfWork.CompleteAsync();

                // Fetch the updated document with user details
                var updatedDocument = await _unitOfWork.Documents.GetByIdWithUserAsync(id);

                return new ApiResponse<DocumentDto>(MapToDto(updatedDocument), "Document updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<DocumentDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteDocumentAsync(Guid id)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdAsync(id);
                
                if (document == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"Document with ID {id} not found");

                _unitOfWork.Documents.Delete(document);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<PagedApiResponse<DocumentDto>> GetPagedDocumentsAsync(PaginationParameters parameters)
        {
            try
            {
                var (documents, totalCount) = await _unitOfWork.Documents.GetPagedAsync(
                    parameters,
                    orderBy: query => parameters.SortBy?.ToLower() switch
                    {
                        "title" => parameters.SortAscending ? query.OrderBy(d => d.Title) : query.OrderByDescending(d => d.Title),
                        "type" => parameters.SortAscending ? query.OrderBy(d => d.DocumentType) : query.OrderByDescending(d => d.DocumentType),
                        "created" => parameters.SortAscending ? query.OrderBy(d => d.CreatedDate) : query.OrderByDescending(d => d.CreatedDate),
                        _ => parameters.SortAscending ? query.OrderBy(d => d.CreatedDate) : query.OrderByDescending(d => d.CreatedDate)
                    },
                    includeProperties: "User"
                );

                var documentDtos = documents.Select(MapToDto).ToList();

                return new PagedApiResponse<DocumentDto>(
                    documentDtos,
                    totalCount,
                    parameters.PageNumber,
                    parameters.PageSize
                );
            }
            catch (Exception ex)
            {
                return new PagedApiResponse<DocumentDto>
                {
                    Success = false,
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        private DocumentDto MapToDto(Document document)
        {
            return new DocumentDto
            {
                Id = document.Id,
                Title = document.Title,
                Content = document.Content,
                DocumentType = document.DocumentType,
                CreatedDate = document.CreatedDate,
                CreatedBy = document.CreatedBy,
                CreatedByName = document.User != null ? $"{document.User.FirstName} {document.User.LastName}" : string.Empty
            };
        }
    }
}