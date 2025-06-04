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
    public class BloodGroupsController : BaseApiController
    {
        private readonly IBloodGroupService _bloodGroupService;

        public BloodGroupsController(IBloodGroupService bloodGroupService)
        {
            _bloodGroupService = bloodGroupService;
        }

        // GET: api/BloodGroups
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodGroupDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodGroups()
        {
            var response = await _bloodGroupService.GetAllBloodGroupsAsync();
            return HandleResponse(response);
        }

        // GET: api/BloodGroups/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodGroup(Guid id)
        {
            var response = await _bloodGroupService.GetBloodGroupByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/BloodGroups
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostBloodGroup([FromBody] CreateBloodGroupDto bloodGroupDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodGroupDto>(ModelState));
            }

            var response = await _bloodGroupService.CreateBloodGroupAsync(bloodGroupDto);
            return HandleResponse(response);
        }

        // PUT: api/BloodGroups/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BloodGroupDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutBloodGroup(Guid id, [FromBody] UpdateBloodGroupDto bloodGroupDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodGroupDto>(ModelState));
            }

            var response = await _bloodGroupService.UpdateBloodGroupAsync(id, bloodGroupDto);
            return HandleResponse(response);
        }

        // DELETE: api/BloodGroups/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteBloodGroup(Guid id)
        {
            var response = await _bloodGroupService.DeleteBloodGroupAsync(id);
            return HandleResponse(response);
        }
    }
}