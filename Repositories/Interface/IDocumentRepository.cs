using BusinessObjects.Models;
using Repositories.Base;
using System;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IDocumentRepository : IGenericRepository<Document>
    {
        Task<Document> GetByIdWithUserAsync(Guid id);
        Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Document>> GetByDocumentTypeAsync(string documentType);
    }
}