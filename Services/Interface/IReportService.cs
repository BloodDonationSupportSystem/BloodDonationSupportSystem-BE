using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IReportService
    {
        /// <summary>
        /// T?o báo cáo hi?n máu trong kho?ng th?i gian
        /// </summary>
        /// <param name="startDate">Th?i gian b?t ??u</param>
        /// <param name="endDate">Th?i gian k?t thúc</param>
        /// <param name="bloodGroupId">ID nhóm máu (tùy ch?n)</param>
        /// <param name="locationId">ID ??a ?i?m (tùy ch?n)</param>
        Task<ApiResponse<BloodDonationReportDto>> GetBloodDonationReportAsync(
            DateTimeOffset startDate, 
            DateTimeOffset endDate, 
            Guid? bloodGroupId = null, 
            Guid? locationId = null);

        /// <summary>
        /// T?o báo cáo yêu c?u máu trong kho?ng th?i gian
        /// </summary>
        /// <param name="startDate">Th?i gian b?t ??u</param>
        /// <param name="endDate">Th?i gian k?t thúc</param>
        /// <param name="bloodGroupId">ID nhóm máu (tùy ch?n)</param>
        /// <param name="locationId">ID ??a ?i?m (tùy ch?n)</param>
        Task<ApiResponse<BloodRequestReportDto>> GetBloodRequestReportAsync(
            DateTimeOffset startDate, 
            DateTimeOffset endDate, 
            Guid? bloodGroupId = null, 
            Guid? locationId = null);

        /// <summary>
        /// T?o báo cáo t?n kho
        /// </summary>
        /// <param name="asOfDate">Th?i ?i?m báo cáo (m?c ??nh là hi?n t?i)</param>
        Task<ApiResponse<InventoryReportDto>> GetInventoryReportAsync(DateTimeOffset? asOfDate = null);

        /// <summary>
        /// T?o báo cáo nhân kh?u h?c ng??i hi?n máu
        /// </summary>
        Task<ApiResponse<DonorDemographicsReportDto>> GetDonorDemographicsReportAsync();

        /// <summary>
        /// Xu?t báo cáo sang ??nh d?ng file
        /// </summary>
        /// <param name="reportParameters">Tham s? báo cáo</param>
        Task<ApiResponse<string>> ExportReportAsync(ReportParameters reportParameters);
    }
}