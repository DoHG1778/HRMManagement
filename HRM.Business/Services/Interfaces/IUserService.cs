using HRM.Business.Common;
using HRM.Business.DTOs.Users;

namespace HRM.Business.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<PagedResult<UserResponseDto>>> GetUsersAsync(
            string? keyword,
            bool? isActive,
            int pageNumber,
            int pageSize);

        Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId);

        Task<ApiResponse<UserResponseDto>> CreateUserAsync(
            int currentUserId,
            CreateUserRequestDto request);

        Task<ApiResponse<UserResponseDto>> UpdateUserAsync(
            int currentUserId,
            int userId,
            UpdateUserRequestDto request);

        Task<ApiResponse<bool>> LockUserAsync(
            int currentUserId,
            int userId);

        Task<ApiResponse<bool>> UnlockUserAsync(
            int currentUserId,
            int userId);

        Task<ApiResponse<bool>> AssignRolesAsync(
            int currentUserId,
            AssignRoleRequestDto request);

        Task<ApiResponse<List<RoleResponseDto>>> GetRolesAsync();
    }
}