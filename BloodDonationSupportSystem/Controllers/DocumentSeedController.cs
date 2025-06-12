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
    [Authorize] // Default authorization for all endpoints
    public class DocumentSeedController : BaseApiController
    {
        private readonly IDocumentSeedService _documentSeedService;

        public DocumentSeedController(IDocumentSeedService documentSeedService)
        {
            _documentSeedService = documentSeedService;
        }

        // POST: api/DocumentSeed/blood-compatibility/{adminUserId}
        [HttpPost("blood-compatibility/{adminUserId}")]
        [Authorize(Roles = "Admin")] // Only Admin can seed documents
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> SeedBloodCompatibilityDocuments(Guid adminUserId)
        {
            var response = await _documentSeedService.SeedBloodCompatibilityDocumentsAsync(adminUserId);
            return HandleResponse(response);
        }
    }
}