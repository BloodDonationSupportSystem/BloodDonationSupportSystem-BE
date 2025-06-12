using BusinessObjects.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;
using Shared.Models;
using System;
using System.Threading.Tasks;

namespace BloodDonationSupportSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // M?c ??nh yêu c?u ??ng nh?p cho t?t c? các endpoints
    public class ReportsController : BaseApiController
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: api/Reports/donation?startDate=2023-01-01&endDate=2023-12-31
        [HttpGet("donation")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem báo cáo hi?n máu
        [ProducesResponseType(typeof(ApiResponse<BloodDonationReportDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonationReport(
            [FromQuery] DateTimeOffset startDate,
            [FromQuery] DateTimeOffset endDate,
            [FromQuery] Guid? bloodGroupId = null,
            [FromQuery] Guid? locationId = null)
        {
            var response = await _reportService.GetBloodDonationReportAsync(startDate, endDate, bloodGroupId, locationId);
            return HandleResponse(response);
        }

        // GET: api/Reports/request?startDate=2023-01-01&endDate=2023-12-31
        [HttpGet("request")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem báo cáo yêu c?u máu
        [ProducesResponseType(typeof(ApiResponse<BloodRequestReportDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetRequestReport(
            [FromQuery] DateTimeOffset startDate,
            [FromQuery] DateTimeOffset endDate,
            [FromQuery] Guid? bloodGroupId = null,
            [FromQuery] Guid? locationId = null)
        {
            var response = await _reportService.GetBloodRequestReportAsync(startDate, endDate, bloodGroupId, locationId);
            return HandleResponse(response);
        }

        // GET: api/Reports/inventory?asOfDate=2023-12-31
        [HttpGet("inventory")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem báo cáo t?n kho
        [ProducesResponseType(typeof(ApiResponse<InventoryReportDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetInventoryReport([FromQuery] DateTimeOffset? asOfDate = null)
        {
            var response = await _reportService.GetInventoryReportAsync(asOfDate);
            return HandleResponse(response);
        }

        // GET: api/Reports/donor-demographics
        [HttpGet("donor-demographics")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xem báo cáo nhân kh?u h?c ng??i hi?n máu
        [ProducesResponseType(typeof(ApiResponse<DonorDemographicsReportDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> GetDonorDemographicsReport()
        {
            var response = await _reportService.GetDonorDemographicsReportAsync();
            return HandleResponse(response);
        }

        // POST: api/Reports/export
        [HttpPost("export")]
        [Authorize(Roles = "Admin,Staff")] // Ch? Admin và Staff có quy?n xu?t báo cáo
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse), 400)]
        [ProducesResponseType(typeof(ApiResponse), 401)]
        [ProducesResponseType(typeof(ApiResponse), 403)]
        [ProducesResponseType(typeof(ApiResponse), 500)]
        public async Task<IActionResult> ExportReport([FromBody] ReportParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return HandleResponse(HandleValidationErrors<string>(ModelState));
            }

            var response = await _reportService.ExportReportAsync(parameters);
            return HandleResponse(response);
        }
    }
}