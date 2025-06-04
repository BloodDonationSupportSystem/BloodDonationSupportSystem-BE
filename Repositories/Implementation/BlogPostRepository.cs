using BusinessObjects.Data;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Interface;
using System;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class BlogPostRepository : GenericRepository<BlogPost>, IBlogPostRepository
    {
        public BlogPostRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<BlogPost> GetByIdWithAuthorAsync(Guid id)
        {
            return await _dbSet
                .Include(bp => bp.User)
                .FirstOrDefaultAsync(bp => bp.Id == id);
        }

        public override async Task<BlogPost> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(bp => bp.User)
                .FirstOrDefaultAsync(bp => bp.Id == id);
        }
    }
}