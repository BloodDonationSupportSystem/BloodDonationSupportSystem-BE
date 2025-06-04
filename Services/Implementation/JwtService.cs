using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repositories.Base;
using Services.Interface;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq; // Add this for LINQ methods

namespace Services.Implementation
{
    public class JwtService : IJwtService
    {
        private readonly JwtConfig _jwtConfig;
        private readonly IUnitOfWork _unitOfWork;

        public JwtService(IOptions<JwtConfig> jwtConfig, IUnitOfWork unitOfWork)
        {
            _jwtConfig = jwtConfig.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<TokenResponseDto> GenerateTokenAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Make sure we have a valid role name
            string roleName = "Unknown";
            
            try
            {
                // Try to get the role name from the user object
                if (user.Role != null && !string.IsNullOrEmpty(user.Role.RoleName))
                {
                    roleName = user.Role.RoleName;
                }
                // If not available, try to load it from the database
                else if (user.RoleId != Guid.Empty)
                {
                    var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);
                    if (role != null && !string.IsNullOrEmpty(role.RoleName))
                    {
                        roleName = role.RoleName;
                        // Update the user's Role reference for consistency
                        user.Role = role;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but continue with default role name
                System.Diagnostics.Debug.WriteLine($"Error loading role: {ex.Message}");
                // Continue with the default role name
            }

            // Create token expiration date
            var tokenExpiration = DateTimeOffset.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes);
            
            // Create claims for the token - ensure no null values
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Role, roleName)
            };
            
            // Create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // Create token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = tokenExpiration.DateTime,
                SigningCredentials = creds,
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience
            };
            
            // Create token handler and generate token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            // Generate refresh token
            var refreshToken = GenerateRefreshToken();
            var refreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationDays);
            
            // Store refresh token in memory instead of database
            TokenStorage.StoreRefreshToken(refreshToken, user.Id, refreshTokenExpiry);
            
            // Create token response with safe values
            return new TokenResponseDto
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = tokenExpiration,
                User = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    LastLogin = user.LastLogin,
                    RoleId = user.RoleId,
                    RoleName = roleName,
                    IsEmailVerified = true
                }
            };
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                throw new SecurityTokenException("Invalid access token");
            }

            // Replace FindFirstValue with a direct claim lookup using LINQ
            var subClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
            if (subClaim == null)
            {
                throw new SecurityTokenException("Invalid access token - missing subject claim");
            }
            
            var userId = Guid.Parse(subClaim.Value);
            
            // Validate refresh token from in-memory storage
            var storedRefreshToken = TokenStorage.GetRefreshToken(refreshToken);
            if (storedRefreshToken == null || 
                storedRefreshToken.UserId != userId || 
                storedRefreshToken.IsUsed || 
                storedRefreshToken.IsRevoked || 
                storedRefreshToken.ExpiryDate <= DateTimeOffset.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            // Mark current refresh token as used
            TokenStorage.MarkRefreshTokenAsUsed(refreshToken);

            // Get user with role included to ensure no null reference issues
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new SecurityTokenException("User not found");
            }
            
            // If the role is not loaded, try to load it
            if (user.Role == null && user.RoleId != Guid.Empty)
            {
                var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);
                if (role != null)
                {
                    user.Role = role;
                }
            }

            // Generate new tokens
            return await GenerateTokenAsync(user);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var storedRefreshToken = TokenStorage.GetRefreshToken(refreshToken);
            if (storedRefreshToken == null)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            TokenStorage.RevokeRefreshToken(refreshToken);
            await Task.CompletedTask; // To keep method signature as async
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret)),
                ValidateLifetime = false, // We're validating an expired token
                ValidIssuer = _jwtConfig.Issuer,
                ValidAudience = _jwtConfig.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}