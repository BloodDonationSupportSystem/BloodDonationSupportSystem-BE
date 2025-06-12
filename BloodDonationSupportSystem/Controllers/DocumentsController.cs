using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // M?c ??nh yêu c?u ??ng nh?p cho t?t c? các endpoints
    public class DocumentsController : BaseApiController
    {
        private readonly IDocumentService _documentService;

        public DocumentsController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        // GET: api/Documents
        [HttpGet]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem danh sách tài li?u
        [ProducesResponseType(typeof(PagedApiResponse<DocumentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDocuments([FromQuery] PaginationParameters parameters)
        {
            var response = await _documentService.GetPagedDocumentsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/Documents/all
        [HttpGet("all")]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem t?t c? tài li?u
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAllDocuments()
        {
            var response = await _documentService.GetAllDocumentsAsync();
            return HandleResponse(response);
        }

        // GET: api/Documents/5
        [HttpGet("{id}")]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem chi ti?t tài li?u
        [ProducesResponseType(typeof(ApiResponse<DocumentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDocument(Guid id)
        {
            var response = await _documentService.GetDocumentByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/Documents/user/5
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p ??u có th? xem tài li?u theo userId
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDocumentsByUser(Guid userId)
        {
            var response = await _documentService.GetDocumentsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/Documents/type/policy
        [HttpGet("type/{documentType}")]
        [AllowAnonymous] // Cho phép ng??i dùng ch?a ??ng nh?p xem tài li?u theo lo?i
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DocumentDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDocumentsByType(string documentType)
        {
            var response = await _documentService.GetDocumentsByTypeAsync(documentType);
            return HandleResponse(response);
        }

        // POST: api/Documents
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n t?o tài li?u m?i
        [ProducesResponseType(typeof(ApiResponse<DocumentDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostDocument([FromBody] CreateDocumentDto documentDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DocumentDto>(ModelState));
            }

            var response = await _documentService.CreateDocumentAsync(documentDto);
            return HandleResponse(response);
        }

        // PUT: api/Documents/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n c?p nh?t tài li?u
        [ProducesResponseType(typeof(ApiResponse<DocumentDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutDocument(Guid id, [FromBody] UpdateDocumentDto documentDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DocumentDto>(ModelState));
            }

            var response = await _documentService.UpdateDocumentAsync(id, documentDto);
            return HandleResponse(response);
        }

        // DELETE: api/Documents/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa tài li?u
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            var response = await _documentService.DeleteDocumentAsync(id);
            return HandleResponse(response);
        }
    }
}