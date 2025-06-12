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
    public class EmergencyRequestsController : BaseApiController
    {
        private readonly IEmergencyRequestService _emergencyRequestService;

        public EmergencyRequestsController(IEmergencyRequestService emergencyRequestService)
        {
            _emergencyRequestService = emergencyRequestService;
        }

        // GET: api/EmergencyRequests
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n xem t?t c? các yêu c?u máu kh?n c?p
        [ProducesResponseType(typeof(PagedApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetEmergencyRequests([FromQuery] EmergencyRequestParameters parameters)
        {
            var response = await _emergencyRequestService.GetPagedEmergencyRequestsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/EmergencyRequests/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p có th? xem chi ti?t yêu c?u máu kh?n c?p
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetEmergencyRequest(Guid id)
        {
            var response = await _emergencyRequestService.GetEmergencyRequestByIdAsync(id);
            return HandleResponse(response);
        }

        // POST: api/EmergencyRequests
        [HttpPost]
        [Authorize(Roles = "Admin,Staff,Member")] // Admin, Staff và Member có quy?n t?o yêu c?u máu kh?n c?p
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostEmergencyRequest([FromBody] CreateEmergencyRequestDto emergencyRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<EmergencyRequestDto>(ModelState));
            }

            var response = await _emergencyRequestService.CreateEmergencyRequestAsync(emergencyRequestDto);
            return HandleResponse(response);
        }

        // PUT: api/EmergencyRequests/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n c?p nh?t yêu c?u máu kh?n c?p
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PutEmergencyRequest(Guid id, [FromBody] UpdateEmergencyRequestDto emergencyRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<EmergencyRequestDto>(ModelState));
            }

            var response = await _emergencyRequestService.UpdateEmergencyRequestAsync(id, emergencyRequestDto);
            return HandleResponse(response);
        }

        // DELETE: api/EmergencyRequests/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Ch? Admin m?i có quy?n xóa yêu c?u máu kh?n c?p
        [ProducesResponseType(typeof(ApiResponse), 204)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteEmergencyRequest(Guid id)
        {
            var response = await _emergencyRequestService.DeleteEmergencyRequestAsync(id);
            return HandleResponse(response);
        }
        
        // GET: api/EmergencyRequests/nearby
        [HttpGet("nearby")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p có th? tìm ki?m yêu c?u máu kh?n c?p g?n ?ó
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmergencyRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetNearbyEmergencyRequests([FromQuery] NearbyEmergencyRequestSearchDto searchDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<IEnumerable<EmergencyRequestDto>>(ModelState));
            }
            
            var response = await _emergencyRequestService.GetEmergencyRequestsByDistanceAsync(
                searchDto.Latitude, searchDto.Longitude, searchDto.RadiusKm,
                searchDto.BloodGroupId, searchDto.UrgencyLevel, searchDto.IsActive);
            
            return HandleResponse(response);
        }
        
        // GET: api/EmergencyRequests/nearby/paged
        [HttpGet("nearby/paged")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p có th? tìm ki?m yêu c?u máu kh?n c?p g?n ?ó (phân trang)
        [ProducesResponseType(typeof(PagedApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPagedNearbyEmergencyRequests(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] double radiusKm = 10.0,
            [FromQuery] EmergencyRequestParameters parameters = null)
        {
            if (radiusKm <= 0 || radiusKm > 500)
            {
                return HandleResponse(new ApiResponse(
                    HttpStatusCode.BadRequest,
                    "Radius must be between 0.1 and 500 kilometers"));
            }
            
            parameters ??= new EmergencyRequestParameters();
            var response = await _emergencyRequestService.GetPagedEmergencyRequestsByDistanceAsync(
                latitude, longitude, radiusKm, parameters);
            
            return HandleResponse(response);
        }
        
        // POST: api/EmergencyRequests/public
        [HttpPost("public")]
        [AllowAnonymous] // Endpoint công khai, không c?n ??ng nh?p
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> PostPublicEmergencyRequest([FromBody] PublicEmergencyRequestDto publicRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<EmergencyRequestDto>(ModelState));
            }
            
            var response = await _emergencyRequestService.CreatePublicEmergencyRequestAsync(publicRequestDto);
            return HandleResponse(response);
        }
        
        // GET: api/EmergencyRequests/active
        [HttpGet("active")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p có th? xem các yêu c?u máu kh?n c?p ?ang ho?t ??ng
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmergencyRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetActiveEmergencyRequests([FromQuery] Guid? bloodGroupId = null)
        {
            var response = await _emergencyRequestService.GetActiveEmergencyRequestsAsync(bloodGroupId);
            return HandleResponse(response);
        }
        
        // GET: api/EmergencyRequests/bloodgroup/{bloodGroupId}
        [HttpGet("bloodgroup/{bloodGroupId}")]
        [Authorize(Roles = "Admin,Staff,Member")] // T?t c? ng??i dùng ?ã ??ng nh?p có th? xem các yêu c?u máu kh?n c?p theo nhóm máu
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<EmergencyRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetEmergencyRequestsByBloodGroup(Guid bloodGroupId, [FromQuery] bool onlyActive = true)
        {
            var response = await _emergencyRequestService.GetEmergencyRequestsByBloodGroupAsync(bloodGroupId, onlyActive);
            return HandleResponse(response);
        }
        
        // PATCH: api/EmergencyRequests/{id}/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n c?p nh?t tr?ng thái yêu c?u máu kh?n c?p
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateEmergencyRequestStatus(Guid id, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return HandleResponse(new ApiResponse(HttpStatusCode.BadRequest, "Status cannot be empty"));
            }
            
            var response = await _emergencyRequestService.UpdateEmergencyRequestStatusAsync(id, status);
            return HandleResponse(response);
        }
        
        // PATCH: api/EmergencyRequests/{id}/inactive
        [HttpPatch("{id}/inactive")]
        [Authorize(Roles = "Admin,Staff")] // Admin và Staff có quy?n ?ánh d?u yêu c?u máu kh?n c?p là không còn ho?t ??ng
        [ProducesResponseType(typeof(ApiResponse<EmergencyRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> MarkEmergencyRequestInactive(Guid id)
        {
            var response = await _emergencyRequestService.MarkEmergencyRequestInactiveAsync(id);
            return HandleResponse(response);
        }
    }
}