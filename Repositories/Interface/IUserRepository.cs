using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
    }
}