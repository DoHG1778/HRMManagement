using HRM.Business.Common;
using HRM.Business.DTOs.Auth;
using HRM.Business.DTOs.Users;
using HRM.Business.Helpers;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtHelper _jwtHelper;

        public AuthService(
            IUnitOfWork unitOfWork,
            JwtHelper jwtHelper)
        {
            _unitOfWork = unitOfWork;
            _jwtHelper = jwtHelper;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(
            int currentUserId,
            ChangePasswordRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<UserResponseDto>> GetMyAccountAsync(int currentUserId)
        {
            throw new NotImplementedException();
        }
    }
}