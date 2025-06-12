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
    public class BloodCompatibilityController : BaseApiController
    {
        private readonly IBloodCompatibilityService _bloodCompatibilityService;

        public BloodCompatibilityController(IBloodCompatibilityService bloodCompatibilityService)
        {
            _bloodCompatibilityService = bloodCompatibilityService;
        }

        // GET: api/BloodCompatibility/wholeblood/{bloodGroupId}
        [HttpGet("wholeblood/{bloodGroupId}")]
        [AllowAnonymous] // Allow anonymous access for compatibility lookups
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodGroupDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetCompatibleBloodGroupsForWholeBlood(Guid bloodGroupId)
        {
            var response = await _bloodCompatibilityService.GetCompatibleBloodGroupsForWholeBloodAsync(bloodGroupId);
            return HandleResponse(response);
        }

        // GET: api/BloodCompatibility/component/{bloodGroupId}/{componentTypeId}
        [HttpGet("component/{bloodGroupId}/{componentTypeId}")]
        [AllowAnonymous] // Allow anonymous access for compatibility lookups
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodGroupDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetCompatibleBloodGroupsForComponent(Guid bloodGroupId, Guid componentTypeId)
        {
            var response = await _bloodCompatibilityService.GetCompatibleBloodGroupsForComponentAsync(bloodGroupId, componentTypeId);
            return HandleResponse(response);
        }

        // GET: api/BloodCompatibility/donors/wholeblood/{bloodGroupId}
        [HttpGet("donors/wholeblood/{bloodGroupId}")]
        [Authorize(Roles = "Admin,Staff,Member")] // Restrict donor lookup to authenticated users
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetCompatibleDonorsForWholeBlood(Guid bloodGroupId, [FromQuery] bool? emergencyOnly = false)
        {
            var response = await _bloodCompatibilityService.GetCompatibleDonorsForWholeBloodAsync(bloodGroupId, emergencyOnly);
            return HandleResponse(response);
        }

        // GET: api/BloodCompatibility/donors/component/{bloodGroupId}/{componentTypeId}
        [HttpGet("donors/component/{bloodGroupId}/{componentTypeId}")]
        [Authorize(Roles = "Admin,Staff,Member")] // Restrict donor lookup to authenticated users
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetCompatibleDonorsForComponent(Guid bloodGroupId, Guid componentTypeId, [FromQuery] bool? emergencyOnly = false)
        {
            var response = await _bloodCompatibilityService.GetCompatibleDonorsForComponentAsync(bloodGroupId, componentTypeId, emergencyOnly);
            return HandleResponse(response);
        }
    }
}