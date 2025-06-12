using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize] // M?c ??nh yêu c?u ??ng nh?p cho t?t c? các endpoints
    public class BloodRequestsController : BaseApiController
    {
        private readonly IBloodRequestService _bloodRequestService;

        public BloodRequestsController(IBloodRequestService bloodRequestService)
        {
            _bloodRequestService = bloodRequestService;
        }

        // GET: api/BloodRequests
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n xem t?t c? các yêu c?u máu
        [ProducesResponseType(typeof(PagedApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodRequests([FromQuery] BloodRequestParameters parameters)
        {
            var response = await _bloodRequestService.GetPagedBloodRequestsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/BloodRequests/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p có th? xem chi ti?t yêu c?u máu
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodRequest(Guid id)
        {
            var response = await _bloodRequestService.GetBloodRequestByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/BloodRequests/nearby
        [HttpGet("nearby")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p có th? tìm ki?m yêu c?u máu g?n ?ó
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNearbyBloodRequests(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] double radiusKm = 10.0, 
            [FromQuery] Guid? bloodGroupId = null, 
            [FromQuery] string status = null)
        {
            if (radiusKm <= 0 || radiusKm > 500)
            {
                return HandleResponse(new ApiResponse(
                    HttpStatusCode.BadRequest,
                    "Radius must be between 0.1 and 500 kilometers"));
            }

            var response = await _bloodRequestService.GetBloodRequestsByDistanceAsync(
                latitude, longitude, radiusKm, bloodGroupId, status);
            return HandleResponse(response);
        }

        // GET: api/BloodRequests/nearby/paged
        [HttpGet("nearby/paged")]
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n xem t?t c? các yêu c?u máu g?n ?ó (phân trang)
        [ProducesResponseType(typeof(PagedApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedNearbyBloodRequests(
            [FromQuery] double latitude, 
            [FromQuery] double longitude, 
            [FromQuery] double radiusKm = 10.0, 
            [FromQuery] BloodRequestParameters parameters = null)
        {
            if (radiusKm <= 0 || radiusKm > 500)
            {
                return HandleResponse(new ApiResponse(
                    HttpStatusCode.BadRequest,
                    "Radius must be between 0.1 and 500 kilometers"));
            }

            parameters ??= new BloodRequestParameters();
            var response = await _bloodRequestService.GetPagedBloodRequestsByDistanceAsync(
                latitude, longitude, radiusKm, parameters);
            return HandleResponse(response);
        }

        // POST: api/BloodRequests
        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p có th? t?o yêu c?u máu
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostBloodRequest([FromBody] CreateBloodRequestDto bloodRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodRequestDto>(ModelState));
            }

            var response = await _bloodRequestService.CreateBloodRequestAsync(bloodRequestDto);
            return HandleResponse(response);
        }

        // PUT: api/BloodRequests/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n c?p nh?t yêu c?u máu
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutBloodRequest(Guid id, [FromBody] UpdateBloodRequestDto bloodRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodRequestDto>(ModelState));
            }

            var response = await _bloodRequestService.UpdateBloodRequestAsync(id, bloodRequestDto);
            return HandleResponse(response);
        }

        // DELETE: api/BloodRequests/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa yêu c?u máu
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteBloodRequest(Guid id)
        {
            var response = await _bloodRequestService.DeleteBloodRequestAsync(id);
            return HandleResponse(response);
        }
    }
}