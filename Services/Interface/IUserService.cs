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
        Task<ApiResponse<UserDto>> AuthenticateAsync(UserLoginDto loginDto);
        Task<ApiResponse> ChangePasswordAsync(Guid userId, ChangePasswordDto passwordDto);
    }
}