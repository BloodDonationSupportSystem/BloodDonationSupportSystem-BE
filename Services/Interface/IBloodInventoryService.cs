using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IBloodInventoryService
    {
        Task<PagedApiResponse<BloodInventoryDto>> GetPagedBloodInventoriesAsync(BloodInventoryParameters parameters);
        Task<ApiResponse<BloodInventoryDto>> GetBloodInventoryByIdAsync(int id);
        Task<ApiResponse<BloodInventoryDto>> CreateBloodInventoryAsync(CreateBloodInventoryDto bloodInventoryDto);
        Task<ApiResponse<BloodInventoryDto>> UpdateBloodInventoryAsync(int id, UpdateBloodInventoryDto bloodInventoryDto);
        Task<ApiResponse> DeleteBloodInventoryAsync(int id);
        Task<ApiResponse<IEnumerable<BloodInventoryDto>>> GetExpiredInventoryAsync();
        Task<ApiResponse<int>> GetAvailableQuantityAsync(Guid bloodGroupId, Guid componentTypeId);
        Task<ApiResponse> UpdateInventoryStatusAsync(int id, string newStatus);
    }
}