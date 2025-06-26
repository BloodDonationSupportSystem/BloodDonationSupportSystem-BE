using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DonationAppointmentRequestsController : BaseApiController
    {
        private readonly IDonationAppointmentRequestService _appointmentRequestService;

        public DonationAppointmentRequestsController(IDonationAppointmentRequestService appointmentRequestService)
        {
            _appointmentRequestService = appointmentRequestService;
        }

        #region Get Appointment Requests

        // GET: api/DonationAppointmentRequests
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(PagedApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAppointmentRequests([FromQuery] AppointmentRequestParameters parameters)
        {
            var response = await _appointmentRequestService.GetPagedAppointmentRequestsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/DonationAppointmentRequests/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Member")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAppointmentRequest(Guid id)
        {
            var response = await _appointmentRequestService.GetAppointmentRequestByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/DonationAppointmentRequests/my-requests
        [HttpGet("my-requests")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationAppointmentRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetMyAppointmentRequests()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<IEnumerable<DonationAppointmentRequestDto>>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.GetAppointmentRequestsByDonorIdAsync(userId);
            return HandleResponse(response);
        }

        // GET: api/DonationAppointmentRequests/pending-staff-reviews
        [HttpGet("pending-staff-reviews")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationAppointmentRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPendingStaffReviews()
        {
            var response = await _appointmentRequestService.GetPendingStaffReviewsAsync();
            return HandleResponse(response);
        }

        // GET: api/DonationAppointmentRequests/pending-donor-responses
        [HttpGet("pending-donor-responses")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationAppointmentRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPendingDonorResponses()
        {
            var response = await _appointmentRequestService.GetPendingDonorResponsesAsync();
            return HandleResponse(response);
        }

        // GET: api/DonationAppointmentRequests/urgent
        [HttpGet("urgent")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationAppointmentRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetUrgentRequests()
        {
            var response = await _appointmentRequestService.GetUrgentRequestsAsync();
            return HandleResponse(response);
        }

        #endregion

        #region Donor-Initiated Requests

        // POST: api/DonationAppointmentRequests/donor-request
        [HttpPost("donor-request")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 409)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreateDonorAppointmentRequest([FromBody] CreateDonorAppointmentRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.CreateDonorAppointmentRequestAsync(requestDto, userId);
            return HandleResponse(response);
        }

        // PUT: api/DonationAppointmentRequests/donor-request/5
        [HttpPut("donor-request/{id}")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateDonorAppointmentRequest(Guid id, [FromBody] UpdateAppointmentRequestDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.UpdateDonorAppointmentRequestAsync(id, updateDto, userId);
            return HandleResponse(response);
        }

        // DELETE: api/DonationAppointmentRequests/donor-request/5
        [HttpDelete("donor-request/{id}")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CancelDonorAppointmentRequest(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.CancelDonorAppointmentRequestAsync(id, userId);
            return HandleResponse(response);
        }

        #endregion

        #region Staff-Initiated Requests (Assignments)

        // POST: api/DonationAppointmentRequests/staff-assignment
        [HttpPost("staff-assignment")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreateStaffAppointmentAssignment([FromBody] CreateStaffAppointmentRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.CreateStaffAppointmentRequestAsync(requestDto, userId);
            return HandleResponse(response);
        }

        // PUT: api/DonationAppointmentRequests/staff-assignment/5
        [HttpPut("staff-assignment/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateStaffAppointmentAssignment(Guid id, [FromBody] UpdateAppointmentRequestDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.UpdateStaffAppointmentRequestAsync(id, updateDto, userId);
            return HandleResponse(response);
        }

        #endregion

        #region Staff Responses to Donor Requests

        // POST: api/DonationAppointmentRequests/5/approve
        [HttpPost("{id}/approve")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ApproveAppointmentRequest(Guid id, [FromBody] StaffAppointmentResponseDto responseDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            responseDto.RequestId = id;
            responseDto.Action = "Approve";

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.ApproveAppointmentRequestAsync(id, responseDto, userId);
            return HandleResponse(response);
        }

        // POST: api/DonationAppointmentRequests/5/reject
        [HttpPost("{id}/reject")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> RejectAppointmentRequest(Guid id, [FromBody] StaffAppointmentResponseDto responseDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            responseDto.RequestId = id;
            responseDto.Action = "Reject";

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.RejectAppointmentRequestAsync(id, responseDto, userId);
            return HandleResponse(response);
        }

        // POST: api/DonationAppointmentRequests/5/modify
        [HttpPost("{id}/modify")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ModifyAppointmentRequest(Guid id, [FromBody] StaffAppointmentResponseDto responseDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            responseDto.RequestId = id;
            responseDto.Action = "Modify";

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.ModifyAppointmentRequestAsync(id, responseDto, userId);
            return HandleResponse(response);
        }

        #endregion

        #region Donor Responses to Staff Assignments

        // POST: api/DonationAppointmentRequests/5/accept
        [HttpPost("{id}/accept")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> AcceptStaffAssignment(Guid id, [FromBody] DonorAppointmentResponseDto responseDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            responseDto.RequestId = id;
            responseDto.Accepted = true;

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.AcceptStaffAssignmentAsync(id, responseDto, userId);
            return HandleResponse(response);
        }

        // POST: api/DonationAppointmentRequests/5/decline
        [HttpPost("{id}/decline")]
        [Authorize(Roles = "Member")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeclineStaffAssignment(Guid id, [FromBody] DonorAppointmentResponseDto responseDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationAppointmentRequestDto>(ModelState));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            responseDto.RequestId = id;
            responseDto.Accepted = false;

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.RejectStaffAssignmentAsync(id, responseDto, userId);
            return HandleResponse(response);
        }

        #endregion

        #region Time Slots and Availability

        // GET: api/DonationAppointmentRequests/available-slots
        [HttpGet("available-slots")]
        [Authorize(Roles = "Admin,Staff,Member")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AvailableTimeSlotsDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAvailableTimeSlots(
            [FromQuery] Guid locationId,
            [FromQuery] DateTimeOffset startDate,
            [FromQuery] int days = 7)
        {
            if (locationId == Guid.Empty)
            {
                return HandleResponse(new ApiResponse<IEnumerable<AvailableTimeSlotsDto>>(
                    System.Net.HttpStatusCode.BadRequest,
                    "Location ID is required"));
            }

            if (days <= 0 || days > 30)
            {
                return HandleResponse(new ApiResponse<IEnumerable<AvailableTimeSlotsDto>>(
                    System.Net.HttpStatusCode.BadRequest,
                    "Days must be between 1 and 30"));
            }

            var response = await _appointmentRequestService.GetAvailableTimeSlotsAsync(locationId, startDate, days);
            return HandleResponse(response);
        }

        // GET: api/DonationAppointmentRequests/location/{locationId}/date/{date}
        [HttpGet("location/{locationId}/date/{date}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationAppointmentRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetAppointmentsByLocationAndDate(Guid locationId, DateTimeOffset date)
        {
            var response = await _appointmentRequestService.GetAppointmentsByLocationAndDateAsync(locationId, date);
            return HandleResponse(response);
        }

        #endregion

        #region Workflow Integration

        // POST: api/DonationAppointmentRequests/5/convert-to-workflow
        [HttpPost("{id}/convert-to-workflow")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationAppointmentRequestDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ConvertToWorkflow(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return HandleResponse(new ApiResponse<DonationAppointmentRequestDto>(
                    System.Net.HttpStatusCode.Unauthorized,
                    "User not authenticated"));
            }

            var userId = Guid.Parse(userIdClaim.Value);
            var response = await _appointmentRequestService.ConvertToWorkflowAsync(id, userId);
            return HandleResponse(response);
        }

        #endregion

        #region Maintenance

        // POST: api/DonationAppointmentRequests/mark-expired
        [HttpPost("mark-expired")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> MarkExpiredRequests()
        {
            var response = await _appointmentRequestService.MarkExpiredRequestsAsync();
            return HandleResponse(response);
        }

        // GET: api/DonationAppointmentRequests/expiring/{hours}
        [HttpGet("expiring/{hours}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationAppointmentRequestDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRequestsExpiringInHours(int hours)
        {
            var response = await _appointmentRequestService.GetRequestsExpiringInHoursAsync(hours);
            return HandleResponse(response);
        }

        #endregion
    }
}