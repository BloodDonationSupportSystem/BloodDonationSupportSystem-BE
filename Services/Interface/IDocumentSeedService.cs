using Shared.Models;
using System;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IDocumentSeedService
    {
        Task<ApiResponse> SeedBloodCompatibilityDocumentsAsync(Guid adminUserId);
    }
}