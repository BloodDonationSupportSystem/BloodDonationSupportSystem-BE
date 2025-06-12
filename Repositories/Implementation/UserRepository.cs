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
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public override async Task<User> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        
        public async Task<IEnumerable<User>> GetUsersByRoleIdAsync(Guid roleId)
        {
            return await _dbSet
                .Include(u => u.Role)
                .Where(u => u.RoleId == roleId)
                .ToListAsync();
        }
    }
}