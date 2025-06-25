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
    [Authorize]
    public class BloodDonationWorkflowsController : BaseApiController
    {
        private readonly IBloodDonationWorkflowService _workflowService;

        public BloodDonationWorkflowsController(IBloodDonationWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        // GET: api/BloodDonationWorkflows
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(PagedApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetWorkflows([FromQuery] DonationWorkflowParameters parameters)
        {
            var response = await _workflowService.GetPagedDonationWorkflowsAsync(parameters);
            return HandleResponse(response);
        }

        // GET: api/BloodDonationWorkflows/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,Donor")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetWorkflow(Guid id)
        {
            var response = await _workflowService.GetDonationWorkflowByIdAsync(id);
            return HandleResponse(response);
        }

        // GET: api/BloodDonationWorkflows/request/{requestId}
        [HttpGet("request/{requestId}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationWorkflowDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetWorkflowsByRequest(Guid requestId, [FromQuery] string requestType)
        {
            var response = await _workflowService.GetDonationWorkflowsByRequestAsync(requestId, requestType);
            return HandleResponse(response);
        }

        // GET: api/BloodDonationWorkflows/donor/{donorId}
        [HttpGet("donor/{donorId}")]
        [Authorize(Roles = "Admin,Staff,Donor")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationWorkflowDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetWorkflowsByDonor(Guid donorId)
        {
            var response = await _workflowService.GetDonationWorkflowsByDonorAsync(donorId);
            return HandleResponse(response);
        }

        // GET: api/BloodDonationWorkflows/status/{status}
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationWorkflowDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetWorkflowsByStatus(string status)
        {
            var response = await _workflowService.GetWorkflowsByStatusAsync(status);
            return HandleResponse(response);
        }

        // GET: api/BloodDonationWorkflows/pending-appointments
        [HttpGet("pending-appointments")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<DonationWorkflowDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetPendingAppointments([FromQuery] DateTimeOffset? startDate, [FromQuery] DateTimeOffset? endDate)
        {
            var response = await _workflowService.GetPendingAppointmentsAsync(startDate, endDate);
            return HandleResponse(response);
        }

        // POST: api/BloodDonationWorkflows
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CreateWorkflow([FromBody] CreateDonationWorkflowDto workflowDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationWorkflowDto>(ModelState));
            }

            var response = await _workflowService.CreateDonationWorkflowAsync(workflowDto);
            return HandleResponse(response);
        }

        // PUT: api/BloodDonationWorkflows/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateWorkflow(Guid id, [FromBody] UpdateDonationWorkflowDto workflowDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationWorkflowDto>(ModelState));
            }

            var response = await _workflowService.UpdateDonationWorkflowAsync(id, workflowDto);
            return HandleResponse(response);
        }

        // POST: api/BloodDonationWorkflows/status
        [HttpPost("status")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> UpdateStatus([FromBody] WorkflowStatusUpdateDto statusUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationWorkflowDto>(ModelState));
            }

            var response = await _workflowService.UpdateWorkflowStatusAsync(statusUpdateDto);
            return HandleResponse(response);
        }

        // POST: api/BloodDonationWorkflows/assign-donor
        [HttpPost("assign-donor")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> AssignDonor([FromBody] AssignDonorDto assignDonorDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationWorkflowDto>(ModelState));
            }

            var response = await _workflowService.AssignDonorAsync(assignDonorDto);
            return HandleResponse(response);
        }

        // POST: api/BloodDonationWorkflows/fulfill-from-inventory
        [HttpPost("fulfill-from-inventory")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> FulfillFromInventory([FromBody] FulfillFromInventoryDto fulfillDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationWorkflowDto>(ModelState));
            }

            var response = await _workflowService.FulfillFromInventoryAsync(fulfillDto);
            return HandleResponse(response);
        }

        // POST: api/BloodDonationWorkflows/complete-donation
        [HttpPost("complete-donation")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CompleteDonation([FromBody] CompleteDonationDto completeDonationDto)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<DonationWorkflowDto>(ModelState));
            }

            var response = await _workflowService.CompleteDonationAsync(completeDonationDto);
            return HandleResponse(response);
        }

        // POST: api/BloodDonationWorkflows/{id}/confirm-appointment
        [HttpPost("{id}/confirm-appointment")]
        [Authorize(Roles = "Admin,Staff,Donor")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ConfirmAppointment(Guid id)
        {
            var response = await _workflowService.ConfirmAppointmentAsync(id);
            return HandleResponse(response);
        }

        // POST: api/BloodDonationWorkflows/{id}/cancel
        [HttpPost("{id}/cancel")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(ApiResponse<DonationWorkflowDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> CancelWorkflow(Guid id, [FromBody] string reason)
        {
            var response = await _workflowService.CancelDonationWorkflowAsync(id, reason);
            return HandleResponse(response);
        }

        // DELETE: api/BloodDonationWorkflows/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 404)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> DeleteWorkflow(Guid id)
        {
            var response = await _workflowService.DeleteDonationWorkflowAsync(id);
            return HandleResponse(response);
        }
    }
}