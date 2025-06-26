using BusinessObjects.Dtos;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interface
{
    public interface IUserService
    {
        Task<ApiResponse<IEnumerable<UserDto>>> GetAllUsersAsync();
        Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid id);
        Task<ApiResponse<UserDto>> GetUserByUsernameAsync(string username);
        Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto userDto);
        Task<ApiResponse<UserDto>> UpdateUserAsync(Guid id, UpdateUserDto userDto);
        Task<ApiResponse> DeleteUserAsync(Guid id);
        Task<ApiResponse<TokenResponseDto>> AuthenticateAsync(UserLoginDto loginDto);
        Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<ApiResponse> RevokeTokenAsync(string refreshToken);
        Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordDto passwordDto);
        
        // Registration methods (simplified)
        Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterUserDto registerDto);
        Task<ApiResponse<UserDto>> RegisterUserAsync(RegisterUserDto registerDto, string roleName);
        Task<ApiResponse<UserDto>> RegisterStaffWithLocationAsync(RegisterStaffWithLocationDto dto);
        
        // Password reset (simplified)
        Task<ApiResponse> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        
        // Kept for backward compatibility but they do nothing now
        Task<ApiResponse> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<ApiResponse> VerifyEmailAsync(VerifyEmailDto verifyEmailDto);
        Task<ApiResponse> ResendVerificationEmailAsync(string email);

        Task<ApiResponse<IEnumerable<StaffWithLocationsDto>>> GetStaffUsersWithLocationsAsync();
        Task<ApiResponse<IEnumerable<UserDto>>> GetMemberUsersAsync();
    }
}