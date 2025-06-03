using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IBloodGroupRepository : IGenericRepository<BloodGroup>
    {
        Task<BloodGroup> GetByNameAsync(string name);
    }
}