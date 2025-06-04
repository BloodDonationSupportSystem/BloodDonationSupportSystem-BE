using BusinessObjects.Models;
using Repositories.Base;
using System.Threading.Tasks;

namespace Repositories.Interface
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken> GetByTokenAsync(string token);
    }
}