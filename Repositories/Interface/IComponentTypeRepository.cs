using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IComponentTypeRepository : IGenericRepository<ComponentType>
    {
        Task<ComponentType> GetByNameAsync(string name);
    }
}