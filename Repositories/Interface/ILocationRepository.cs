using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface ILocationRepository : IGenericRepository<Location>
    {
        Task<Location> GetByNameAsync(string name);
    }
}