using BusinessObjects.Dtos;
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
    public class DonorProfilesController : BaseApiController
    {
        private readonly IDonorProfileService _donorProfileService;

        public DonorProfilesController(IDonorProfileService donorProfileService)
        {
            _donorProfileService = donorProfileService;
        }

        // GET: api/DonorProfiles
        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfiles([FromQuery] DonorProfileParameters parameters)
        {
            var response = await _donorProfileService.GetPagedDonorProfilesAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/all
        [HttpGet("all")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAllDonorProfiles()
        {
            var response = await _donorProfileService.GetAllDonorProfilesAsync();
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfile(Guid id)
        {
            var response = await _donorProfileService.GetDonorProfileByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/user/5
        [HttpGet("user/{userId}")]
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfileByUserId(Guid userId)
        {
            var response = await _donorProfileService.GetDonorProfileByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/DonorProfiles/bloodgroup/5
        [HttpGet("bloodgroup/{bloodGroupId}")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonorProfileDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorProfilesByBloodGroupId(Guid bloodGroupId)
        {
            var response = await _donorProfileService.GetDonorProfilesByBloodGroupIdAsync(bloodGroupId);
            return HandleResponse(response);
        }

        // POST: api/DonorProfiles
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostDonorProfile([FromBody] CreateDonorProfileDto donorProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonorProfileDto>(ModelState));
            }

            var response = await _donorProfileService.CreateDonorProfileAsync(donorProfileDto);
            return HandleResponse(response);
        }

        // PUT: api/DonorProfiles/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<DonorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutDonorProfile(Guid id, [FromBody] UpdateDonorProfileDto donorProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonorProfileDto>(ModelState));
            }

            var response = await _donorProfileService.UpdateDonorProfileAsync(id, donorProfileDto);
            return HandleResponse(response);
        }

        // DELETE: api/DonorProfiles/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteDonorProfile(Guid id)
        {
            var response = await _donorProfileService.DeleteDonorProfileAsync(id);
            return HandleResponse(response);
        }
    }
}