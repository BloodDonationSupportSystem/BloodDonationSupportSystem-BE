using AutoMapper;
using BusinessObjects.Dtos;
using Microsoft.Extensions.Logging;
using Repositories.Base;
using Services.Interface;
using Shared.Models;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<BloodDonationReportDto>> GetBloodDonationReportAsync(
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            Guid? bloodGroupId = null,
            Guid? locationId = null)
        {
            try
            {
                var report = await _unitOfWork.Analytics.GetBloodDonationReportAsync(startDate, endDate, bloodGroupId, locationId);
                return new ApiResponse<BloodDonationReportDto>(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating blood donation report");
                return new ApiResponse<BloodDonationReportDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while generating blood donation report");
            }
        }

        public async Task<ApiResponse<BloodRequestReportDto>> GetBloodRequestReportAsync(
            DateTimeOffset startDate,
            DateTimeOffset endDate,
            Guid? bloodGroupId = null,
            Guid? locationId = null)
        {
            try
            {
                var report = await _unitOfWork.Analytics.GetBloodRequestReportAsync(startDate, endDate, bloodGroupId, locationId);
                return new ApiResponse<BloodRequestReportDto>(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating blood request report");
                return new ApiResponse<BloodRequestReportDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while generating blood request report");
            }
        }

        public async Task<ApiResponse<InventoryReportDto>> GetInventoryReportAsync(DateTimeOffset? asOfDate = null)
        {
            try
            {
                var report = await _unitOfWork.Analytics.GetInventoryReportAsync(asOfDate);
                return new ApiResponse<InventoryReportDto>(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating inventory report");
                return new ApiResponse<InventoryReportDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while generating inventory report");
            }
        }

        public async Task<ApiResponse<DonorDemographicsReportDto>> GetDonorDemographicsReportAsync()
        {
            try
            {
                var report = await _unitOfWork.Analytics.GetDonorDemographicsReportAsync();
                return new ApiResponse<DonorDemographicsReportDto>(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating donor demographics report");
                return new ApiResponse<DonorDemographicsReportDto>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while generating donor demographics report");
            }
        }

        public async Task<ApiResponse<string>> ExportReportAsync(ReportParameters reportParameters)
        {
            try
            {
                // Ki?m tra ??u vào
                if (reportParameters == null)
                {
                    return new ApiResponse<string>(
                        HttpStatusCode.BadRequest,
                        "Report parameters are required");
                }

                if (string.IsNullOrEmpty(reportParameters.ReportType))
                {
                    return new ApiResponse<string>(
                        HttpStatusCode.BadRequest,
                        "Report type is required");
                }

                if (string.IsNullOrEmpty(reportParameters.Format))
                {
                    return new ApiResponse<string>(
                        HttpStatusCode.BadRequest,
                        "Export format is required");
                }

                // ??t giá tr? m?c ??nh cho th?i gian n?u không có
                reportParameters.StartDate ??= DateTimeOffset.UtcNow.AddMonths(-1);
                reportParameters.EndDate ??= DateTimeOffset.UtcNow;

                // L?y d? li?u báo cáo d?a trên lo?i báo cáo
                object reportData = null;
                string reportTitle = "";

                switch (reportParameters.ReportType.ToLower())
                {
                    case "donation":
                        var donationReport = await _unitOfWork.Analytics.GetBloodDonationReportAsync(
                            reportParameters.StartDate.Value,
                            reportParameters.EndDate.Value,
                            reportParameters.BloodGroupId,
                            reportParameters.LocationId);
                        reportData = donationReport;
                        reportTitle = "Báo cáo hi?n máu";
                        break;

                    case "request":
                        var requestReport = await _unitOfWork.Analytics.GetBloodRequestReportAsync(
                            reportParameters.StartDate.Value,
                            reportParameters.EndDate.Value,
                            reportParameters.BloodGroupId,
                            reportParameters.LocationId);
                        reportData = requestReport;
                        reportTitle = "Báo cáo yêu c?u máu";
                        break;

                    case "inventory":
                        var inventoryReport = await _unitOfWork.Analytics.GetInventoryReportAsync(
                            reportParameters.EndDate);
                        reportData = inventoryReport;
                        reportTitle = "Báo cáo t?n kho máu";
                        break;

                    case "donor":
                        var donorReport = await _unitOfWork.Analytics.GetDonorDemographicsReportAsync();
                        reportData = donorReport;
                        reportTitle = "Báo cáo nhân kh?u h?c ng??i hi?n máu";
                        break;

                    default:
                        return new ApiResponse<string>(
                            HttpStatusCode.BadRequest,
                            "Invalid report type. Valid types are: donation, request, inventory, donor");
                }

                // T?o file báo cáo d?a trên ??nh d?ng
                string exportedFile = "";

                switch (reportParameters.Format.ToLower())
                {
                    case "csv":
                        exportedFile = GenerateCsvReport(reportData, reportTitle);
                        break;

                    case "json":
                        exportedFile = System.Text.Json.JsonSerializer.Serialize(reportData, new System.Text.Json.JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        break;

                    default:
                        return new ApiResponse<string>(
                            HttpStatusCode.BadRequest,
                            "Invalid format. Valid formats are: csv, json");
                }

                return new ApiResponse<string>(exportedFile, "Report generated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting report");
                return new ApiResponse<string>(
                    HttpStatusCode.InternalServerError,
                    "Error occurred while exporting report");
            }
        }

        #region Helper Methods

        private string GenerateCsvReport(object reportData, string reportTitle)
        {
            var sb = new StringBuilder();
            
            // Thêm tiêu ?? báo cáo
            sb.AppendLine(reportTitle);
            sb.AppendLine();
            
            // X? lý t?ng lo?i báo cáo
            if (reportData is BloodDonationReportDto donationReport)
            {
                // Thêm thông tin t?ng quan
                sb.AppendLine($"Th?i gian báo cáo:,{donationReport.StartDate:dd/MM/yyyy} - {donationReport.EndDate:dd/MM/yyyy}");
                sb.AppendLine($"T?ng s? l?n hi?n máu:,{donationReport.TotalDonations}");
                sb.AppendLine($"S? ng??i hi?n máu:,{donationReport.UniqueDonoCount}");
                sb.AppendLine($"Ng??i hi?n máu l?n ??u:,{donationReport.NewDonorCount}");
                sb.AppendLine($"Ng??i hi?n máu l?p l?i:,{donationReport.RepeatDonorCount}");
                sb.AppendLine($"Trung bình m?i ngày:,{donationReport.AverageDonationsPerDay}");
                sb.AppendLine();
                
                // Thêm th?ng kê theo nhóm máu
                sb.AppendLine("Hi?n máu theo nhóm máu:");
                sb.AppendLine("Nhóm máu,S? l??ng,T? l? (%)");
                foreach (var item in donationReport.DonationsByBloodGroup)
                {
                    sb.AppendLine($"{item.BloodGroupName},{item.Count},{item.Percentage}");
                }
                sb.AppendLine();
                
                // Thêm th?ng kê theo thành ph?n
                sb.AppendLine("Hi?n máu theo thành ph?n:");
                sb.AppendLine("Thành ph?n,S? l??ng,T? l? (%)");
                foreach (var item in donationReport.DonationsByComponentType)
                {
                    sb.AppendLine($"{item.ComponentTypeName},{item.Count},{item.Percentage}");
                }
                sb.AppendLine();
                
                // Thêm th?ng kê theo ngày
                sb.AppendLine("Xu h??ng hi?n máu theo ngày:");
                sb.AppendLine("Ngày,S? l??ng");
                foreach (var item in donationReport.DonationTrend)
                {
                    sb.AppendLine($"{item.Date:dd/MM/yyyy},{item.Count}");
                }
            }
            else if (reportData is BloodRequestReportDto requestReport)
            {
                // Thêm thông tin t?ng quan
                sb.AppendLine($"Th?i gian báo cáo:,{requestReport.StartDate:dd/MM/yyyy} - {requestReport.EndDate:dd/MM/yyyy}");
                sb.AppendLine($"T?ng s? yêu c?u:,{requestReport.TotalRequests}");
                sb.AppendLine($"Yêu c?u ?ã ?áp ?ng:,{requestReport.FulfilledRequests}");
                sb.AppendLine($"Yêu c?u ?ang x? lý:,{requestReport.PendingRequests}");
                sb.AppendLine($"Yêu c?u ?ã h?y:,{requestReport.CancelledRequests}");
                sb.AppendLine($"T? l? ?áp ?ng (%):,{requestReport.FulfillmentRate}");
                sb.AppendLine($"Th?i gian ?áp ?ng trung bình (gi?):,{requestReport.AverageFulfillmentTime}");
                sb.AppendLine();
                
                // Thêm th?ng kê theo nhóm máu
                sb.AppendLine("Yêu c?u theo nhóm máu:");
                sb.AppendLine("Nhóm máu,S? l??ng,T? l? (%)");
                foreach (var item in requestReport.RequestsByBloodGroup)
                {
                    sb.AppendLine($"{item.BloodGroupName},{item.Count},{item.Percentage}");
                }
                sb.AppendLine();
                
                // Thêm th?ng kê theo m?c ?? ?u tiên
                sb.AppendLine("Yêu c?u theo m?c ?? ?u tiên:");
                sb.AppendLine("M?c ?? ?u tiên,S? l??ng,T? l? (%)");
                foreach (var item in requestReport.RequestsByPriority)
                {
                    sb.AppendLine($"{item.Priority},{item.Count},{item.Percentage}");
                }
            }
            else if (reportData is InventoryReportDto inventoryReport)
            {
                // Thêm thông tin t?ng quan
                sb.AppendLine($"Th?i ?i?m báo cáo:,{inventoryReport.ReportDate:dd/MM/yyyy HH:mm}");
                sb.AppendLine($"T?ng ??n v? máu kh? d?ng:,{inventoryReport.TotalInventoryUnits}");
                sb.AppendLine($"??n v? máu s?p h?t h?n:,{inventoryReport.ExpiringSoonUnits}");
                sb.AppendLine($"??n v? máu ?ã h?t h?n:,{inventoryReport.ExpiredUnits}");
                sb.AppendLine($"Th?i gian l?u tr? trung bình (ngày):,{inventoryReport.AverageStorageTime}");
                sb.AppendLine($"T? l? h?t h?n (%):,{inventoryReport.ExpirationRate}");
                sb.AppendLine();
                
                // Thêm th?ng kê theo nhóm máu
                sb.AppendLine("T?n kho theo nhóm máu:");
                sb.AppendLine("Nhóm máu,??n v? kh? d?ng,??n v? s?p h?t h?n");
                foreach (var item in inventoryReport.InventoryByBloodGroup)
                {
                    sb.AppendLine($"{item.BloodGroupName},{item.AvailableUnits},{item.ExpiringSoonUnits}");
                }
            }
            else if (reportData is DonorDemographicsReportDto donorReport)
            {
                // Thêm thông tin t?ng quan
                sb.AppendLine($"T?ng s? ng??i hi?n máu:,{donorReport.TotalDonors}");
                sb.AppendLine($"Trung bình s? l?n hi?n máu m?i ng??i:,{donorReport.AverageDonationsPerDonor}");
                sb.AppendLine();
                
                // Thêm th?ng kê theo nhóm tu?i
                sb.AppendLine("Ng??i hi?n máu theo nhóm tu?i:");
                sb.AppendLine("Nhóm tu?i,S? l??ng,T? l? (%)");
                foreach (var item in donorReport.DonorsByAgeGroup)
                {
                    sb.AppendLine($"{item.AgeGroup},{item.Count},{item.Percentage}");
                }
                sb.AppendLine();
                
                // Thêm th?ng kê theo gi?i tính
                sb.AppendLine("Ng??i hi?n máu theo gi?i tính:");
                sb.AppendLine("Gi?i tính,S? l??ng,T? l? (%)");
                foreach (var item in donorReport.DonorsByGender)
                {
                    sb.AppendLine($"{item.GenderName},{item.Count},{item.Percentage}");
                }
                sb.AppendLine();
                
                // Thêm th?ng kê theo nhóm máu
                sb.AppendLine("Ng??i hi?n máu theo nhóm máu:");
                sb.AppendLine("Nhóm máu,S? l??ng,T? l? (%)");
                foreach (var item in donorReport.DonorsByBloodGroup)
                {
                    sb.AppendLine($"{item.BloodGroupName},{item.Count},{item.Percentage}");
                }
            }
            
            return sb.ToString();
        }

        #endregion
    }
}