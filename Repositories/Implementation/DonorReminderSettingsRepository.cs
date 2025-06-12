using BusinessObjects.Data;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class DonorReminderSettingsRepository : GenericRepository<DonorReminderSettings>, IDonorReminderSettingsRepository
    {
        public DonorReminderSettingsRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<DonorReminderSettings> GetByDonorProfileIdAsync(Guid donorProfileId)
        {
            return await _dbSet
                .Include(drs => drs.DonorProfile)
                    .ThenInclude(dp => dp.User)
                .FirstOrDefaultAsync(drs => drs.DonorProfileId == donorProfileId);
        }

        public async Task<IEnumerable<DonorReminderSettings>> GetDonorsNeedingRemindersAsync(int daysBeforeEligible = 7)
        {
            var today = DateTimeOffset.UtcNow.Date;
            
            // L?y danh sách ng??i hi?n máu có th? hi?n máu l?i trong kho?ng th?i gian c? th?
            // và ?ã b?t tính n?ng nh?c nh?
            return await _dbSet
                .Include(drs => drs.DonorProfile)
                    .ThenInclude(dp => dp.User)
                .Where(drs => 
                    drs.EnableReminders && 
                    drs.DonorProfile.NextAvailableDonationDate.HasValue &&
                    (today.AddDays(daysBeforeEligible) >= drs.DonorProfile.NextAvailableDonationDate.Value.Date) &&
                    (
                        // Ch?a bao gi? g?i nh?c nh?
                        !drs.LastReminderSentTime.HasValue ||
                        // Ho?c l?n cu?i g?i nh?c nh? cách ?ây ít nh?t 7 ngày
                        (drs.LastReminderSentTime.HasValue && drs.LastReminderSentTime.Value.AddDays(7) <= today)
                    )
                )
                .ToListAsync();
        }

        public async Task<bool> UpdateLastReminderSentTimeAsync(Guid id)
        {
            var reminderSettings = await _dbSet.FindAsync(id);
            if (reminderSettings == null)
                return false;
                
            reminderSettings.LastReminderSentTime = DateTimeOffset.UtcNow;
            reminderSettings.LastUpdatedTime = DateTimeOffset.UtcNow;
            
            _context.Entry(reminderSettings).State = EntityState.Modified;
            return true;
        }
    }
}