using HRM.Business.Common;
using HRM.Business.DTOs.Users;
using HRM.Business.Helpers;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PagedResult<UserResponseDto>>> GetUsersAsync(
            string? keyword,
            bool? isActive,
            int pageNumber,
            int pageSize)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<UserResponseDto>> CreateUserAsync(
            int currentUserId,
            CreateUserRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateUserAsync(
            int currentUserId,
            int userId,
            UpdateUserRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> LockUserAsync(
            int currentUserId,
            int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> UnlockUserAsync(
            int currentUserId,
            int userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> AssignRolesAsync(
            int currentUserId,
            AssignRoleRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<RoleResponseDto>>> GetRolesAsync()
        {
            throw new NotImplementedException();
        }
    }
}