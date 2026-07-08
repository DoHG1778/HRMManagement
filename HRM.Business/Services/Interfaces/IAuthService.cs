using HRM.Business.Common;
using HRM.Business.DTOs.Auth;
using HRM.Business.DTOs.Users;

namespace HRM.Business.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);

        Task<ApiResponse<bool>> ChangePasswordAsync(
            int currentUserId,
            ChangePasswordRequestDto request);

        Task<ApiResponse<UserResponseDto>> GetMyAccountAsync(int currentUserId);
    }
}