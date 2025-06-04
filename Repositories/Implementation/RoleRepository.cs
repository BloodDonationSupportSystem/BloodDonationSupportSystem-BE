using BusinessObjects.Data;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class RoleRepository : GenericRepository<Role>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Role> GetByNameAsync(string roleName)
        {
            // Load all roles and then filter with case-insensitive comparison
            var roles = await _dbSet.ToListAsync();
            return roles.FirstOrDefault(r => 
                string.Equals(r.RoleName, roleName, StringComparison.OrdinalIgnoreCase));
        }
    }
}