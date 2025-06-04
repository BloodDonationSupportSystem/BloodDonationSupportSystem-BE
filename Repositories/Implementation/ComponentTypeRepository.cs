using BusinessObjects.Data;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class ComponentTypeRepository : GenericRepository<ComponentType>, IComponentTypeRepository
    {
        public ComponentTypeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ComponentType> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(ct => ct.Name == name);
        }
    }
}