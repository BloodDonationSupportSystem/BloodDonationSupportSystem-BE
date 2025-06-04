using BusinessObjects.Data;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class BloodGroupRepository : GenericRepository<BloodGroup>, IBloodGroupRepository
    {
        public BloodGroupRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BloodGroup> GetByNameAsync(string groupName)
        {
            return await _dbSet.FirstOrDefaultAsync(bg => bg.GroupName == groupName);
        }
    }
}