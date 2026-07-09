using HRM.Business.Common;
using HRM.Business.DTOs.Auth;
using HRM.Business.DTOs.Users;
using HRM.Business.Helpers;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.UnitOfWork;
using Microsoft.Extensions.Configuration;

namespace HRM.Business.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtHelper _jwtHelper;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUnitOfWork unitOfWork,
            JwtHelper jwtHelper,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _jwtHelper = jwtHelper;
            _configuration = configuration;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
        {
            if (request == null)
                return ApiResponse<LoginResponseDto>.Fail("Login information is required.");

            if (string.IsNullOrWhiteSpace(request.UsernameOrEmail))
                return ApiResponse<LoginResponseDto>.Fail("Username or email is required.");

            if (string.IsNullOrWhiteSpace(request.Password))
                return ApiResponse<LoginResponseDto>.Fail("Password is required.");

            var usernameOrEmail = request.UsernameOrEmail.Trim();
            var user = await _unitOfWork.Users.GetByUsernameOrEmailAsync(usernameOrEmail);

            if (user == null)
                return ApiResponse<LoginResponseDto>.Fail("Invalid username/email or password.", 401);

            if (!user.IsActive)
                return ApiResponse<LoginResponseDto>.Fail("Your account is inactive or locked.", 403);

            bool isPasswordValid = PasswordHelper.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
                return ApiResponse<LoginResponseDto>.Fail("Invalid username/email or password.", 401);

            var roles = GetActiveRoles(user);
            if (roles.Count == 0)
                return ApiResponse<LoginResponseDto>.Fail("User does not have any valid role.", 403);

            user.LastLoginAt = DateTime.Now;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var currentUser = new CurrentUser
            {
                UserId = user.UserId,
                EmployeeId = user.Employee?.EmployeeId,
                Username = user.Username,
                Email = user.Email,
                Roles = roles
            };

            var token = _jwtHelper.GenerateToken(currentUser);
            var response = new LoginResponseDto
            {
                UserId = user.UserId,
                EmployeeId = user.Employee?.EmployeeId,
                Username = user.Username,
                Email = user.Email,
                Roles = roles,
                Token = token,
                ExpiredAt = DateTime.Now.AddMinutes(GetJwtExpireMinutes())
            };

            return ApiResponse<LoginResponseDto>.Ok(response, "Login successfully.");
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(
            int currentUserId,
            ChangePasswordRequestDto request)
        {
            if (currentUserId <= 0)
                return ApiResponse<bool>.Unauthorized("You must login before changing password.");

            if (request == null)
                return ApiResponse<bool>.Fail("Password information is required.");

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                return ApiResponse<bool>.Fail("Current password is required.");

            if (string.IsNullOrWhiteSpace(request.NewPassword))
                return ApiResponse<bool>.Fail("New password is required.");

            if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
                return ApiResponse<bool>.Fail("Confirm password is required.");

            if (request.NewPassword != request.ConfirmPassword)
                return ApiResponse<bool>.Fail("Confirm password does not match new password.");

            if (!PasswordHelper.IsPasswordStrong(request.NewPassword))
                return ApiResponse<bool>.Fail("New password must be at least 8 characters and include uppercase, lowercase, number and special character.");

            var user = await _unitOfWork.Users.GetByIdAsync(currentUserId);
            if (user == null)
                return ApiResponse<bool>.NotFound("User not found.");

            if (!user.IsActive)
                return ApiResponse<bool>.Fail("Your account is inactive or locked.", 403);

            bool isCurrentPasswordValid = PasswordHelper.VerifyPassword(request.CurrentPassword, user.PasswordHash);
            if (!isCurrentPasswordValid)
                return ApiResponse<bool>.Fail("Current password is incorrect.");

            bool isSamePassword = PasswordHelper.VerifyPassword(request.NewPassword, user.PasswordHash);
            if (isSamePassword)
                return ApiResponse<bool>.Fail("New password must be different from current password.");

            user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.Now;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Password changed successfully.");
        }

        public async Task<ApiResponse<UserResponseDto>> GetMyAccountAsync(int currentUserId)
        {
            if (currentUserId <= 0)
                return ApiResponse<UserResponseDto>.Unauthorized("You must login to view account information.");

            var user = await _unitOfWork.Users.GetUserWithRolesAsync(currentUserId);
            if (user == null)
                return ApiResponse<UserResponseDto>.NotFound("User not found.");

            return ApiResponse<UserResponseDto>.Ok(MapUserToDto(user));
        }

        private static List<string> GetActiveRoles(User user)
        {
            return user.UserRoles
                .Where(ur => ur.Role != null && ur.Role.IsActive)
                .Select(ur => ur.Role.RoleName)
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private int GetJwtExpireMinutes()
        {
            var expireConfig = _configuration["Jwt:ExpireMinutes"];
            if (!string.IsNullOrWhiteSpace(expireConfig) && int.TryParse(expireConfig, out int expireMinutes))
            {
                return expireMinutes;
            }

            return 120;
        }

        private static UserResponseDto MapUserToDto(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = GetActiveRoles(user),
                EmployeeId = user.Employee?.EmployeeId,
                EmployeeCode = user.Employee?.EmployeeCode,
                FullName = user.Employee?.FullName
            };
        }
    }
}
