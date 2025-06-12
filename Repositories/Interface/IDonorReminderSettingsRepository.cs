using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IDonorReminderSettingsRepository : IGenericRepository<DonorReminderSettings>
    {
        /// <summary>
        /// L?y cài ??t nh?c nh? theo ID ng??i hi?n máu
        /// </summary>
        /// <param name="donorProfileId">ID c?a ng??i hi?n máu</param>
        /// <returns>Cài ??t nh?c nh?</returns>
        Task<DonorReminderSettings> GetByDonorProfileIdAsync(Guid donorProfileId);
        
        /// <summary>
        /// L?y danh sách ng??i hi?n máu s?p ?? ?i?u ki?n hi?n máu l?i và c?n ???c nh?c nh?
        /// </summary>
        /// <param name="daysBeforeEligible">S? ngày tr??c khi ?? ?i?u ki?n</param>
        /// <returns>Danh sách cài ??t nh?c nh? c?a nh?ng ng??i hi?n máu c?n ???c nh?c nh?</returns>
        Task<IEnumerable<DonorReminderSettings>> GetDonorsNeedingRemindersAsync(int daysBeforeEligible = 7);
        
        /// <summary>
        /// C?p nh?t th?i gian g?i nh?c nh? g?n nh?t
        /// </summary>
        /// <param name="id">ID c?a cài ??t nh?c nh?</param>
        /// <returns>True n?u c?p nh?t thành công</returns>
        Task<bool> UpdateLastReminderSentTimeAsync(Guid id);
    }
}