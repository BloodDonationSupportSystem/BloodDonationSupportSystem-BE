using BusinessObjects.Data;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class LocationRepository : GenericRepository<Location>, ILocationRepository
    {
        public LocationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Location> GetByNameAsync(string name)
        {
            return await _dbSet.FirstOrDefaultAsync(l => l.Name == name);
        }
    }
}