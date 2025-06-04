using BusinessObjects.Dtos;
using BusinessObjects.Models;
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

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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

                // Reload to get the role
                user = await _unitOfWork.Users.GetByIdAsync(user.Id);

                return new ApiResponse<UserDto>(MapToDto(user), "User created successfully")
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

        public async Task<ApiResponse<UserDto>> AuthenticateAsync(UserLoginDto loginDto)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(loginDto.UserName);
                
                if (user == null)
                    return new ApiResponse<UserDto>(HttpStatusCode.Unauthorized, "Invalid username or password");

                // Verify password
                if (VerifyPassword(loginDto.Password, user.Password))
                {
                    // Update last login time
                    user.LastLogin = DateTimeOffset.UtcNow;
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.CompleteAsync();
                    
                    return new ApiResponse<UserDto>(MapToDto(user), "Authentication successful");
                }

                return new ApiResponse<UserDto>(HttpStatusCode.Unauthorized, "Invalid username or password");
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>(HttpStatusCode.InternalServerError, ex.Message);
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
                RoleName = user.Role?.RoleName ?? "Unknown"
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
    }
}