using BusinessObjects.Dtos;
using BusinessObjects.Models;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IJwtService
    {
        Task<TokenResponseDto> GenerateTokenAsync(User user);
        Task<TokenResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}