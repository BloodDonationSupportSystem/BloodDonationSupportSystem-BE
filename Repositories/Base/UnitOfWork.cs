using BusinessObjects.Data;
using Repositories.Implementation;
using Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace Repositories.Base
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private ILocationRepository _locationRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ILocationRepository Locations => _locationRepository ??= new LocationRepository(_context);

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}