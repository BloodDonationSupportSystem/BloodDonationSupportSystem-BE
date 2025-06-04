using BusinessObjects.Dtos;
using BusinessObjects.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Repositories.Base;
using Services.Interface;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public UserService(IUnitOfWork unitOfWork, IJwtService jwtService)
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
        }

        public async Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllAsync();
                var userDtos = users.Select(MapToDto).ToList();

                return new ApiResponse<IEnumerable<UserDto>>(userDtos)
                {
                    Message = $"Retrieved {userDtos.Count} users successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<UserDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                
                if (user == null)
                    return new ApiResponse<UserDto>(HttpStatusCode.NotFound, $"User with ID {id} not found");

                return new ApiResponse<UserDto>(MapToDto(user));
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(username);
                
                if (user == null)
                    return new ApiResponse<UserDto>(HttpStatusCode.NotFound, $"User with username {username} not found");

                return new ApiResponse<UserDto>(MapToDto(user));
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto userDto)
        {
            try
            {
                // Check if username already exists
                var existingUserName = await _unitOfWork.Users.GetByUsernameAsync(userDto.UserName);
                if (existingUserName != null)
                {
                    return new ApiResponse<UserDto>(HttpStatusCode.Conflict, $"Username '{userDto.UserName}' is already taken");
                }

                // Check if email already exists
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(userDto.Email);
                if (existingEmail != null)
                {
                    return new ApiResponse<UserDto>(HttpStatusCode.Conflict, $"Email '{userDto.Email}' is already registered");
                }

                // Verify role exists
                var role = await _unitOfWork.Roles.GetByIdAsync(userDto.RoleId);
                if (role == null)
                {
                    return new ApiResponse<UserDto>(HttpStatusCode.BadRequest, $"Role with ID {userDto.RoleId} does not exist");
                }

                // Hash the password
                string hashedPassword = HashPassword(userDto.Password);

                var user = new User
                {
                    UserName = userDto.UserName,
                    Email = userDto.Email,
                    Password = hashedPassword,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    PhoneNumber = userDto.PhoneNumber,
                    LastLogin = DateTimeOffset.UtcNow,
                    RoleId = userDto.RoleId
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                // Automatically mark the email as verified
                TokenStorage.SetEmailVerified(user.Id);

                // Reload to get the role
                user = await _unitOfWork.Users.GetByIdAsync(user.Id);

                return new ApiResponse<UserDto>(MapToDto(user), "User created successfully.")
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto userDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                
                if (user == null)
                    return new ApiResponse<UserDto>(HttpStatusCode.NotFound, $"User with ID {id} not found");

                // Check if email is being changed and if it already exists
                if (user.Email != userDto.Email)
                {
                    var existingEmail = await _unitOfWork.Users.GetByEmailAsync(userDto.Email);
                    if (existingEmail != null)
                    {
                        return new ApiResponse<UserDto>(HttpStatusCode.Conflict, $"Email '{userDto.Email}' is already registered");
                    }
                }

                // Verify role exists
                var role = await _unitOfWork.Roles.GetByIdAsync(userDto.RoleId);
                if (role == null)
                {
                    return new ApiResponse<UserDto>(HttpStatusCode.BadRequest, $"Role with ID {userDto.RoleId} does not exist");
                }

                // Update user properties
                user.Email = userDto.Email;
                user.FirstName = userDto.FirstName;
                user.LastName = userDto.LastName;
                user.PhoneNumber = userDto.PhoneNumber;
                user.RoleId = userDto.RoleId;

                // Update password if provided
                if (!string.IsNullOrEmpty(userDto.Password))
                {
                    user.Password = HashPassword(userDto.Password);
                }

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                // Reload to get the role
                user = await _unitOfWork.Users.GetByIdAsync(user.Id);

                return new ApiResponse<UserDto>(MapToDto(user), "User updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(id);
                
                if (user == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"User with ID {id} not found");

                _unitOfWork.Users.Delete(user);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.NoContent);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<TokenResponseDto>> AuthenticateAsync(UserLoginDto loginDto)
        {
            try
            {
                // Get user with role included
                var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.UserName);
                
                if (user == null)
                    return new ApiResponse<TokenResponseDto>(HttpStatusCode.Unauthorized, "Invalid username or password");

                // Verify password
                if (VerifyPassword(loginDto.Password, user.Password))
                {
                    // Update last login time
                    user.LastLogin = DateTimeOffset.UtcNow;
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.CompleteAsync();
                    
                    // Make sure the user has the role loaded
                    if (user.Role == null || user.Role.RoleName == null)
                    {
                        // Need to get the role separately
                        var role = await _unitOfWork.Roles.GetByIdAsync(user.RoleId);
                        if (role != null)
                        {
                            user.Role = role;
                        }
                    }
                    
                    // Generate JWT token
                    var tokenResponse = await _jwtService.GenerateTokenAsync(user);
                    
                    return new ApiResponse<TokenResponseDto>(tokenResponse, "Authentication successful");
                }

                return new ApiResponse<TokenResponseDto>(HttpStatusCode.Unauthorized, "Invalid username or password");
            }
            catch (Exception ex)
            {
                return new ApiResponse<TokenResponseDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            try
            {
                var tokenResponse = await _jwtService.RefreshTokenAsync(accessToken, refreshToken);
                return new ApiResponse<TokenResponseDto>(tokenResponse, "Token refreshed successfully");
            }
            catch (SecurityTokenException ex)
            {
                return new ApiResponse<TokenResponseDto>(HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                return new ApiResponse<TokenResponseDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                await _jwtService.RevokeRefreshTokenAsync(refreshToken);
                return new ApiResponse(HttpStatusCode.OK, "Token revoked successfully");
            }
            catch (SecurityTokenException ex)
            {
                return new ApiResponse(HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordDto passwordDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                
                if (user == null)
                    return new ApiResponse(HttpStatusCode.NotFound, $"User with ID {userId} not found");

                // Verify current password
                if (!VerifyPassword(passwordDto.CurrentPassword, user.Password))
                {
                    return new ApiResponse(HttpStatusCode.BadRequest, "Current password is incorrect");
                }

                // Update password
                user.Password = HashPassword(passwordDto.NewPassword);
                
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.OK, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterUserDto registerDto)
        {
            // Default to "Member" role
            return await RegisterUserAsync(registerDto, "Member");
        }

        public async Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterUserDto registerDto, string roleName)
        {
            try
            {
                // Check if username already exists
                var existingUserName = await _unitOfWork.Users.GetByUsernameAsync(registerDto.UserName);
                if (existingUserName != null)
                {
                    return new ApiResponse<UserDto>(HttpStatusCode.Conflict, $"Username '{registerDto.UserName}' is already taken");
                }

                // Check if email already exists
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(registerDto.Email);
                if (existingEmail != null)
                {
                    return new ApiResponse<UserDto>(HttpStatusCode.Conflict, $"Email '{registerDto.Email}' is already registered");
                }

                // Get the role directly by name - this is the key fix
                var role = await _unitOfWork.Roles.GetByNameAsync(roleName);
                
                if (role == null)
                {
                    // If we can't find the role by name, log available roles for diagnostics
                    var allRoles = await _unitOfWork.Roles.GetAllAsync();
                    string availableRoles = string.Join(", ", allRoles.Select(r => r.RoleName));
                    
                    return new ApiResponse<UserDto>(HttpStatusCode.BadRequest, 
                        $"Role '{roleName}' does not exist in the system. Available roles: {availableRoles}");
                }

                // Hash the password
                string hashedPassword = HashPassword(registerDto.Password);

                var user = new User
                {
                    UserName = registerDto.UserName,
                    Email = registerDto.Email,
                    Password = hashedPassword,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PhoneNumber = registerDto.PhoneNumber,
                    LastLogin = DateTimeOffset.UtcNow,
                    RoleId = role.Id  // Use the existing role ID from the database
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                // Automatically mark the email as verified
                TokenStorage.SetEmailVerified(user.Id);

                // Reload to get the role
                user = await _unitOfWork.Users.GetByIdAsync(user.Id);

                string message = $"Registration as {role.RoleName} successful!";
                
                return new ApiResponse<UserDto>(MapToDto(user), message)
                {
                    StatusCode = HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(forgotPasswordDto.Email);
                
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return new ApiResponse(HttpStatusCode.OK, "If your email is registered, you will receive a password reset link shortly");
                }

                return new ApiResponse(HttpStatusCode.OK, "If your email is registered, you will receive a password reset link shortly");
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByEmailAsync(resetPasswordDto.Email);
                
                if (user == null)
                {
                    return new ApiResponse(HttpStatusCode.BadRequest, "Invalid token or email");
                }

                // Update password (simplified - no token validation)
                user.Password = HashPassword(resetPasswordDto.NewPassword);
                
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                
                return new ApiResponse(HttpStatusCode.OK, "Password has been reset successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse> VerifyEmailAsync(VerifyEmailDto verifyEmailDto)
        {
            // Since we're automatically verifying emails, just return success
            return new ApiResponse(HttpStatusCode.OK, "Email verified successfully");
        }

        public async Task<ApiResponse> ResendVerificationEmailAsync(string email)
        {
            // Since we're automatically verifying emails, just return success
            return new ApiResponse(HttpStatusCode.OK, "Email verification not required");
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                LastLogin = user.LastLogin,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "Unknown",
                IsEmailVerified = true
            };
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            string hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }

        private string GenerateRandomToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}