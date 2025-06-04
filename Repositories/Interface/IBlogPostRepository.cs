using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IBlogPostRepository : IGenericRepository<BlogPost>
    {
        Task<BlogPost> GetByIdWithAuthorAsync(Guid id);
    }
}