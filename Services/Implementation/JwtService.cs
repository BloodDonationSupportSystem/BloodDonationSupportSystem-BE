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
using System.Linq;

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
            }

            var now = DateTime.UtcNow;
            var expiration = now.AddMinutes(_jwtConfig.AccessTokenExpirationMinutes);
            
            // Create claims for the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
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
                NotBefore = now,
                IssuedAt = now,
                Expires = expiration,
                SigningCredentials = creds,
                Issuer = _jwtConfig.Issuer,
                Audience = _jwtConfig.Audience
            };
            
            // Create token handler and generate token
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            // Generate refresh token and store in database
            var refreshTokenString = GenerateRefreshToken();
            var refreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(_jwtConfig.RefreshTokenExpirationDays);
            
            // Store refresh token in database
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshTokenString,
                ExpiryDate = refreshTokenExpiry,
                UserId = user.Id,
                IsUsed = false,
                IsRevoked = false
            };
            
            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.CompleteAsync();
            
            // Create token response
            return new TokenResponseDto
            {
                AccessToken = tokenHandler.WriteToken(token),
                RefreshToken = refreshTokenString,
                Expiration = new DateTimeOffset(expiration),
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

            var subClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (subClaim == null)
            {
                throw new SecurityTokenException("Invalid access token - missing subject claim");
            }
            
            var userId = Guid.Parse(subClaim.Value);
            
            // Validate refresh token from database
            var storedRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
            if (storedRefreshToken == null || 
                storedRefreshToken.UserId != userId || 
                storedRefreshToken.IsUsed || 
                storedRefreshToken.IsRevoked || 
                storedRefreshToken.ExpiryDate <= DateTimeOffset.UtcNow)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            // Mark current refresh token as used in database
            storedRefreshToken.IsUsed = true;
            _unitOfWork.RefreshTokens.Update(storedRefreshToken);
            await _unitOfWork.CompleteAsync();

            // Get user with role included
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
            var storedRefreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);
            if (storedRefreshToken == null)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            storedRefreshToken.IsRevoked = true;
            _unitOfWork.RefreshTokens.Update(storedRefreshToken);
            await _unitOfWork.CompleteAsync();
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64]; // Increased to 64 bytes for better security
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
                ValidAudience = _jwtConfig.Audience,
                ClockSkew = TimeSpan.Zero
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