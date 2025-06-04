using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IBloodInventoryRepository : IGenericRepository<BloodInventory>
    {
        Task<(IEnumerable<BloodInventory> items, int totalCount)> GetPagedBloodInventoriesAsync(BloodInventoryParameters parameters);
        Task<BloodInventory> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<BloodInventory>> GetExpiredInventoryAsync();
        Task<IEnumerable<BloodInventory>> GetByBloodGroupAndComponentTypeAsync(Guid bloodGroupId, Guid componentTypeId);
        Task<int> GetAvailableQuantityAsync(Guid bloodGroupId, Guid componentTypeId);
    }
}