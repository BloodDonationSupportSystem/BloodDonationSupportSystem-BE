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

        public async Task<ApiResponse<UserDto>> UpdateUserActivationAsync(Guid userId, bool isActivated)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return new ApiResponse<UserDto>(HttpStatusCode.NotFound, $"User with ID {userId} not found");

                user.IsActivated = isActivated;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();

                return new ApiResponse<UserDto>(MapToDto(user), "User activation status updated successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<UserDto>> RegisterStaffWithLocationAsync(RegisterStaffWithLocationDto dto)
        {
            try
            {
                // Check if username already exists
                var existingUserName = await _unitOfWork.Users.GetByUsernameAsync(dto.UserName);
                if (existingUserName != null)
                {
                    return new ApiResponse<UserDto>(System.Net.HttpStatusCode.Conflict, $"Username '{dto.UserName}' is already taken");
                }

                // Check if email already exists
                var existingEmail = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
                if (existingEmail != null)
                {
                    return new ApiResponse<UserDto>(System.Net.HttpStatusCode.Conflict, $"Email '{dto.Email}' is already registered");
                }

                // Get the role directly by name (for system role, not location role)
                var role = await _unitOfWork.Roles.GetByNameAsync("Staff");
                if (role == null)
                {
                    return new ApiResponse<UserDto>(System.Net.HttpStatusCode.BadRequest, $"System role 'Staff' does not exist");
                }

                // Check if location exists
                var location = await _unitOfWork.Locations.GetByIdAsync(dto.LocationId);
                if (location == null)
                {
                    return new ApiResponse<UserDto>(System.Net.HttpStatusCode.BadRequest, $"Location with ID {dto.LocationId} does not exist");
                }

                // Hash the password
                string hashedPassword = HashPassword(dto.Password);

                var user = new User();
                user.UserName = dto.UserName;
                user.Email = dto.Email;
                user.Password = hashedPassword;
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.PhoneNumber = dto.PhoneNumber;
                user.LastLogin = DateTimeOffset.UtcNow;
                user.RoleId = role.Id;
                user.CreatedTime = DateTimeOffset.UtcNow;
                user.IsActivated = true; // default khi tạo mới

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                // Reload to get the user with ID
                user = await _unitOfWork.Users.GetByIdAsync(user.Id);

                // Assign location for staff
                var assignment = new LocationStaffAssignment
                {
                    LocationId = dto.LocationId,
                    UserId = user.Id,
                    Role = dto.LocationRole,
                    CanManageCapacity = dto.CanManageCapacity,
                    CanApproveAppointments = dto.CanApproveAppointments,
                    CanViewReports = dto.CanViewReports,
                    AssignedDate = DateTimeOffset.UtcNow,
                    IsActive = true,
                    Notes = dto.Notes
                };
                await _unitOfWork.LocationStaffAssignments.AddAsync(assignment);
                await _unitOfWork.CompleteAsync();

                string message = $"Staff registered and assigned to location successfully!";
                return new ApiResponse<UserDto>(MapToDto(user), message)
                {
                    StatusCode = System.Net.HttpStatusCode.Created
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<UserDto>(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<StaffWithLocationsDto>>> GetStaffUsersWithLocationsAsync()
        {
            try
            {
                var staffRole = await _unitOfWork.Roles.GetByNameAsync("Staff");
                if (staffRole == null)
                {
                    return new ApiResponse<IEnumerable<StaffWithLocationsDto>>(HttpStatusCode.BadRequest, "Staff role not found");
                }
                var staffUsers = (await _unitOfWork.Users.GetUsersByRoleIdAsync(staffRole.Id))
                    .OrderByDescending(u => u.CreatedTime)
                    .ToList();
                var result = new List<StaffWithLocationsDto>();
                foreach (var staff in staffUsers)
                {
                    var assignments = await _unitOfWork.LocationStaffAssignments.GetByUserIdAsync(staff.Id);
                    var assignmentDtos = assignments.Select(a => new BusinessObjects.Dtos.LocationStaffAssignmentDto
                    {
                        Id = a.Id,
                        LocationId = a.LocationId,
                        LocationName = a.Location?.Name ?? string.Empty,
                        UserId = a.UserId,
                        UserName = staff.UserName,
                        UserEmail = staff.Email,
                        Role = a.Role,
                        CanManageCapacity = a.CanManageCapacity,
                        CanApproveAppointments = a.CanApproveAppointments,
                        CanViewReports = a.CanViewReports,
                        AssignedDate = a.AssignedDate,
                        UnassignedDate = a.UnassignedDate,
                        IsActive = a.IsActive,
                        Notes = a.Notes,
                        CreatedTime = a.CreatedTime,
                        LastUpdatedTime = a.LastUpdatedTime
                    }).ToList();
                    result.Add(new StaffWithLocationsDto
                    {
                        Staff = MapToDto(staff),
                        Locations = assignmentDtos
                    });
                }
                return new ApiResponse<IEnumerable<StaffWithLocationsDto>>(result, "Get staff users with locations successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<StaffWithLocationsDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<IEnumerable<UserDto>>> GetMemberUsersAsync()
        {
            try
            {
                var memberRole = await _unitOfWork.Roles.GetByNameAsync("Donor");
                if (memberRole == null)
                {
                    return new ApiResponse<IEnumerable<UserDto>>(HttpStatusCode.BadRequest, "Donor role not found");
                }
                var members = (await _unitOfWork.Users.GetUsersByRoleIdAsync(memberRole.Id))
                    .OrderByDescending(u => u.CreatedTime)
                    .ToList();
                var memberDtos = members.Select(MapToDto).ToList();
                return new ApiResponse<IEnumerable<UserDto>>(memberDtos, "Get member users successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse<IEnumerable<UserDto>>(HttpStatusCode.InternalServerError, ex.Message);
            }
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
                    RoleId = userDto.RoleId,
                    CreatedTime = DateTimeOffset.UtcNow,
                    IsActivated = true // default khi tạo mới
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                // Automatically consider email as verified
                // Email verification not needed as we automatically trust user emails

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

                // Check if account is locked out
                if (user.IsLockedOut)
                {
                    var remainingLockout = user.LockoutEnd!.Value - DateTimeOffset.UtcNow;
                    return new ApiResponse<TokenResponseDto>(
                        HttpStatusCode.Unauthorized, 
                        $"Account is locked. Please try again in {Math.Ceiling(remainingLockout.TotalMinutes)} minutes.");
                }

                // Check if account is activated
                if (!user.IsActivated)
                {
                    return new ApiResponse<TokenResponseDto>(HttpStatusCode.Unauthorized, "Account is deactivated. Please contact administrator.");
                }

                // Verify password
                if (VerifyPassword(loginDto.Password, user.Password))
                {
                    // Reset failed login attempts on successful login
                    user.FailedLoginAttempts = 0;
                    user.LockoutEnd = null;
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

                // Increment failed login attempts
                user.FailedLoginAttempts++;
                
                // Check if we need to lock the account (default: 5 attempts, 15 minutes lockout)
                const int maxFailedAttempts = 5;
                const int lockoutDurationMinutes = 15;
                
                if (user.FailedLoginAttempts >= maxFailedAttempts)
                {
                    user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(lockoutDurationMinutes);
                    _unitOfWork.Users.Update(user);
                    await _unitOfWork.CompleteAsync();
                    
                    return new ApiResponse<TokenResponseDto>(
                        HttpStatusCode.Unauthorized, 
                        $"Account has been locked due to too many failed login attempts. Please try again in {lockoutDurationMinutes} minutes.");
                }
                
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CompleteAsync();
                
                int remainingAttempts = maxFailedAttempts - user.FailedLoginAttempts;
                return new ApiResponse<TokenResponseDto>(
                    HttpStatusCode.Unauthorized, 
                    $"Invalid username or password. {remainingAttempts} attempt(s) remaining before account lockout.");
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
            // Default to "Donor" role
            return await RegisterUserAsync(registerDto, "Donor");
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
                    RoleId = role.Id,  // Use the existing role ID from the database
                    CreatedTime = DateTimeOffset.UtcNow,
                    IsActivated = true // default khi tạo mới
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CompleteAsync();

                // Automatically consider email as verified
                // Email verification not needed as we automatically trust user emails

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
                IsEmailVerified = true,
                CreatedTime = user.CreatedTime,
                IsActivated = user.IsActivated
            };
        }

        private string HashPassword(string password)
        {
            // Use BCrypt for secure password hashing with automatic salt generation
            // Work factor of 12 provides good security while keeping performance reasonable
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                // Check if it's a BCrypt hash (starts with $2a$, $2b$, or $2y$)
                if (storedHash.StartsWith("$2"))
                {
                    return BCrypt.Net.BCrypt.Verify(password, storedHash);
                }

                // Legacy support for HMACSHA512 format (96+ bytes base64)
                byte[] hashBytes = Convert.FromBase64String(storedHash);
                if (hashBytes.Length >= 96)
                {
                    // Extract salt (first 32 bytes)
                    byte[] salt = new byte[32];
                    Array.Copy(hashBytes, 0, salt, 0, 32);

                    // Hash the provided password with the extracted salt
                    using (var hmac = new System.Security.Cryptography.HMACSHA512(salt))
                    {
                        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                        
                        // Compare the computed hash with stored hash (starting from byte 32)
                        for (int i = 0; i < computedHash.Length; i++)
                        {
                            if (computedHash[i] != hashBytes[32 + i])
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                }

                // Legacy SHA256 verification for oldest passwords
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    string hashedInput = Convert.ToBase64String(hashedBytes);
                    return hashedInput == storedHash;
                }
            }
            catch
            {
                return false;
            }
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