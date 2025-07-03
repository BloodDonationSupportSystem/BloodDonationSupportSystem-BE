using BloodDonationSupportSystem.Extensions;
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
        private readonly IRealTimeNotificationService _realTimeNotificationService;

        public BloodRequestsController(
            IBloodRequestService bloodRequestService,
            IRealTimeNotificationService realTimeNotificationService)
        {
            _bloodRequestService = bloodRequestService;
            _realTimeNotificationService = realTimeNotificationService;
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

        // GET: api/BloodRequests/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Staff,Member")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BloodRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetBloodRequestsByUserId(Guid userId)
        {
            // For Members, they should only be able to see their own requests
            if (User.IsInRole("Member") && User.GetUserId() != userId)
            {
                return Forbid();
            }

            var response = await _bloodRequestService.GetBloodRequestsByUserIdAsync(userId);
            return HandleResponse(response);
        }

        // POST: api/BloodRequests/{id}/fulfill-from-inventory
        [HttpPost("{id}/fulfill-from-inventory")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> FulfillFromInventory(Guid id)
        {
            var response = await _bloodRequestService.FulfillBloodRequestFromInventoryAsync(id);

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
            
            // Send real-time notification to staff
            if (response.Success && response.Data != null)
            {
                await _realTimeNotificationService.NotifyStaffOfNewRequest(response.Data);
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
            }

            return HandleResponse(response);
        }

        // POST: api/BloodRequests/emergency/public
        [HttpPost("emergency/public")]
        [AllowAnonymous] // Public emergency requests don't need authentication
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreatePublicEmergencyRequest([FromBody] PublicBloodRequestDto publicRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodRequestDto>(ModelState));
            }

            var response = await _bloodRequestService.CreatePublicBloodRequestAsync(publicRequestDto);
            
            // Send real-time emergency notifications
            if (response.Success && response.Data != null && response.Data.IsEmergency)
            {
                // Create emergency DTO for real-time notifications
                var emergencyDto = new EmergencyBloodRequestDto
                {
                    Id = response.Data.Id,
                    PatientName = response.Data.PatientName,
                    UrgencyLevel = response.Data.UrgencyLevel,
                    ContactInfo = response.Data.ContactInfo,
                    HospitalName = response.Data.HospitalName,
                    QuantityUnits = response.Data.QuantityUnits,
                    BloodGroupId = response.Data.BloodGroupId,
                    BloodGroupName = response.Data.BloodGroupName,
                    ComponentTypeId = response.Data.ComponentTypeId,
                    ComponentTypeName = response.Data.ComponentTypeName,
                    Address = response.Data.Address,
                    Latitude = response.Data.Latitude,
                    Longitude = response.Data.Longitude,
                    MedicalNotes = response.Data.MedicalNotes,
                    Status = response.Data.Status,
                    RequestDate = response.Data.RequestDate,
                    CreatedTime = response.Data.CreatedTime ?? DateTimeOffset.UtcNow
                };

                // Send emergency alerts
                await _realTimeNotificationService.SendEmergencyBloodRequestAlert(emergencyDto);
                await _realTimeNotificationService.UpdateEmergencyDashboard();
                
                // TODO: Find and notify nearby donors
                // var nearbyDonors = await FindNearbyDonors(emergencyDto);
                // await _realTimeNotificationService.NotifyNearbyDonors(emergencyDto, nearbyDonors);
            }

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
            
            // Send real-time status update
            if (response.Success && response.Data != null)
            {
                await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                    id, response.Data.Status, $"Blood request updated to {response.Data.Status}");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                // If it's an emergency request update
                if (response.Data.IsEmergency)
                {
                    await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                        id, response.Data.Status, $"Emergency request status: {response.Data.Status}");
                    await _realTimeNotificationService.UpdateEmergencyDashboard();
                }
            }

            return HandleResponse(response);
        }

        // PUT: api/BloodRequests/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,Staff,Member")]
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateBloodRequestStatus(Guid id, [FromBody] UpdateBloodRequestStatusDto statusDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodRequestDto>(ModelState));
            }

            var response = await _bloodRequestService.UpdateBloodRequestStatusAsync(id, statusDto.Status);
            
            // Send real-time status update
            if (response.Success && response.Data != null)
            {
                await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                    id, response.Data.Status, statusDto.Notes ?? $"Status updated to {response.Data.Status}");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
                
                // Special handling for emergency requests
                if (response.Data.IsEmergency)
                {
                    await _realTimeNotificationService.SendEmergencyBloodRequestUpdate(
                        id, response.Data.Status, statusDto.Notes ?? $"Emergency status: {response.Data.Status}");
                    await _realTimeNotificationService.UpdateEmergencyDashboard();
                }
            }

            return HandleResponse(response);
        }

        // PUT: api/BloodRequests/5/deactivate
        [HttpPut("{id}/deactivate")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeactivateBloodRequest(Guid id)
        {
            var response = await _bloodRequestService.MarkBloodRequestInactiveAsync(id);
            
            // Send real-time update
            if (response.Success && response.Data != null)
            {
                await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                    id, "Deactivated", "Blood request has been deactivated");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
            }

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
            
            // Send real-time update
            if (response.Success)
            {
                await _realTimeNotificationService.NotifyBloodRequestStatusChange(
                    id, "Deleted", "Blood request has been deleted");
                await _realTimeNotificationService.UpdateBloodRequestDashboard();
            }

            return HandleResponse(response);
        }

        // GET: api/BloodRequests/{id}/inventory-check
        [HttpGet("{id}/inventory-check")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<InventoryCheckResultDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CheckInventoryForRequest(Guid id)
        {
            var response = await _bloodRequestService.CheckInventoryForRequestAsync(id);
            return HandleResponse(response);
        }

        // POST: api/BloodRequests/public
        [HttpPost("public")]
        [AllowAnonymous] // Public emergency requests don't need authentication
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreatePublicBloodRequest([FromBody] PublicBloodRequestDto publicRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<BloodRequestDto>(ModelState));
            }

            var response = await _bloodRequestService.CreatePublicBloodRequestAsync(publicRequestDto);
            
            // Send real-time emergency notifications
            if (response.Success && response.Data != null && response.Data.IsEmergency)
            {
                // Create emergency DTO for real-time notifications
                var emergencyDto = new EmergencyBloodRequestDto
                {
                    Id = response.Data.Id,
                    PatientName = response.Data.PatientName,
                    UrgencyLevel = response.Data.UrgencyLevel,
                    ContactInfo = response.Data.ContactInfo,
                    HospitalName = response.Data.HospitalName,
                    QuantityUnits = response.Data.QuantityUnits,
                    BloodGroupId = response.Data.BloodGroupId,
                    BloodGroupName = response.Data.BloodGroupName,
                    ComponentTypeId = response.Data.ComponentTypeId,
                    ComponentTypeName = response.Data.ComponentTypeName,
                    Address = response.Data.Address,
                    Latitude = response.Data.Latitude,
                    Longitude = response.Data.Longitude,
                    MedicalNotes = response.Data.MedicalNotes,
                    Status = response.Data.Status,
                    RequestDate = response.Data.RequestDate,
                    CreatedTime = response.Data.CreatedTime ?? DateTimeOffset.UtcNow
                };

                // Send emergency alerts
                await _realTimeNotificationService.SendEmergencyBloodRequestAlert(emergencyDto);
                await _realTimeNotificationService.UpdateEmergencyDashboard();
            }

            return HandleResponse(response);
        }

        // GET: api/BloodRequests/emergency
        [HttpGet("emergency")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<BloodRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetEmergencyBloodRequests([FromQuery] bool onlyActive = true)
        {
            var response = await _bloodRequestService.GetEmergencyBloodRequestsAsync(onlyActive);
            return HandleResponse(response);
        }
    }
}