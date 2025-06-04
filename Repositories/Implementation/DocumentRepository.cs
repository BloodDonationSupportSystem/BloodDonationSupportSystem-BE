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
    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Document> GetByIdWithUserAsync(Guid id)
        {
            return await _dbSet
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(d => d.User)
                .Where(d => d.CreatedBy == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetByDocumentTypeAsync(string documentType)
        {
            return await _dbSet
                .Include(d => d.User)
                .Where(d => d.DocumentType == documentType)
                .ToListAsync();
        }
    }
}