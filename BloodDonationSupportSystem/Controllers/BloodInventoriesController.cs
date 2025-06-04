using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodInventoriesController : BaseApiController
    {
        private readonly IBloodInventoryService _bloodInventoryService;

        public BloodInventoriesController(IBloodInventoryService bloodInventoryService)
        {
            _bloodInventoryService = bloodInventoryService;
        }

        // GET: api/BloodInventories
        [HttpGet]
        [ProducesResponseType(typeof(PagedApiResponse<BloodInventoryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodInventories([FromQuery] BloodInventoryParameters parameters)
        {
            var response = await _bloodInventoryService.GetPagedBloodInventoriesAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/BloodInventories/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BloodInventoryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodInventory(int id)
        {
            var response = await _bloodInventoryService.GetBloodInventoryByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/BloodInventories/expired
        [HttpGet("expired")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodInventoryDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetExpiredInventory()
        {
            var response = await _bloodInventoryService.GetExpiredInventoryAsync();
            return HandleResponse(response);
        }

        // GET: api/BloodInventories/available-quantity
        [HttpGet("available-quantity")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAvailableQuantity([FromQuery] Guid bloodGroupId, [FromQuery] Guid componentTypeId)
        {
            if (bloodGroupId == Guid.Empty || componentTypeId == Guid.Empty)
            {
                return HandleResponse(new ApiResponse(HttpStatusCode.BadRequest, "Blood group ID and component type ID are required"));
            }
            
            var response = await _bloodInventoryService.GetAvailableQuantityAsync(bloodGroupId, componentTypeId);
            return HandleResponse(response);
        }

        // POST: api/BloodInventories
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BloodInventoryDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostBloodInventory([FromBody] CreateBloodInventoryDto bloodInventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodInventoryDto>(ModelState));
            }

            var response = await _bloodInventoryService.CreateBloodInventoryAsync(bloodInventoryDto);
            return HandleResponse(response);
        }

        // PUT: api/BloodInventories/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponse<BloodInventoryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutBloodInventory(int id, [FromBody] UpdateBloodInventoryDto bloodInventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodInventoryDto>(ModelState));
            }

            var response = await _bloodInventoryService.UpdateBloodInventoryAsync(id, bloodInventoryDto);
            return HandleResponse(response);
        }

        // PATCH: api/BloodInventories/5/status
        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateInventoryStatus(int id, [FromBody] string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                return HandleResponse(new ApiResponse(HttpStatusCode.BadRequest, "Status value is required"));
            }

            var response = await _bloodInventoryService.UpdateInventoryStatusAsync(id, newStatus);
            return HandleResponse(response);
        }

        // DELETE: api/BloodInventories/5
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteBloodInventory(int id)
        {
            var response = await _bloodInventoryService.DeleteBloodInventoryAsync(id);
            return HandleResponse(response);
        }
    }
}