using HRM.Business.Common;
using HRM.Business.DTOs.Users;
using HRM.Business.Helpers;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

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
            var query = _unitOfWork.Users.Query()
                .Include(u => u.Employee)
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(keyword) || u.Email.ToLower().Contains(keyword));
            }

            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responseItems = items.Select(u => MapToResponseDto(u)).ToList();

            var result = PagedResult<UserResponseDto>.Create(responseItems, pageNumber, pageSize, totalCount);
            return ApiResponse<PagedResult<UserResponseDto>>.Ok(result);
        }

        public async Task<ApiResponse<UserResponseDto>> GetUserByIdAsync(int userId)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
            if (user == null)
                return ApiResponse<UserResponseDto>.NotFound("User not found.");

            return ApiResponse<UserResponseDto>.Ok(MapToResponseDto(user));
        }

        public async Task<ApiResponse<UserResponseDto>> CreateUserAsync(
            int currentUserId,
            CreateUserRequestDto request)
        {
            if (request == null)
                return ApiResponse<UserResponseDto>.Fail("Request data is required.");

            if (await _unitOfWork.Users.IsUsernameExistsAsync(request.Username))
                return ApiResponse<UserResponseDto>.Fail($"Username '{request.Username}' is already taken.");

            if (await _unitOfWork.Users.IsEmailExistsAsync(request.Email))
                return ApiResponse<UserResponseDto>.Fail($"Email '{request.Email}' is already registered.");

            if (!PasswordHelper.IsPasswordStrong(request.Password))
                return ApiResponse<UserResponseDto>.Fail("Password is too weak.");

            Employee? employee = null;
            if (request.EmployeeId.HasValue)
            {
                employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId.Value);
                if (employee == null)
                    return ApiResponse<UserResponseDto>.Fail("Target employee not found.");

                if (employee.UserId.HasValue)
                    return ApiResponse<UserResponseDto>.Fail("This employee already has a user account.");
            }

            var user = new User
            {
                Username = request.Username.Trim(),
                Email = request.Email.Trim().ToLower(),
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            if (request.RoleIds != null && request.RoleIds.Any())
            {
                foreach (var roleId in request.RoleIds)
                {
                    user.UserRoles.Add(new UserRole { RoleId = roleId, AssignedAt = DateTime.Now });
                }
            }

            await _unitOfWork.Users.AddAsync(user);
            
            if (employee != null)
            {
                employee.User = user; 
                _unitOfWork.Employees.Update(employee);
            }

            await _unitOfWork.SaveChangesAsync();

            var createdUser = await _unitOfWork.Users.GetUserWithRolesAsync(user.UserId);
            return ApiResponse<UserResponseDto>.Ok(MapToResponseDto(createdUser!), "User created successfully.");
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateUserAsync(
            int currentUserId,
            int userId,
            UpdateUserRequestDto request)
        {
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
            if (user == null) return ApiResponse<UserResponseDto>.NotFound("User not found.");

            if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Users.IsEmailExistsAsync(request.Email))
                    return ApiResponse<UserResponseDto>.Fail("Email is already taken.");
                user.Email = request.Email.Trim().ToLower();
            }

            if (request.IsActive.HasValue)
            {
                if (!request.IsActive.Value && await _unitOfWork.Users.IsLastAdminAsync(userId))
                    return ApiResponse<UserResponseDto>.Fail("Cannot lock the last Admin.");
                user.IsActive = request.IsActive.Value;
            }

            if (request.RoleIds != null)
            {
                // Logic to update roles safely
                bool willHaveAdmin = false;
                foreach(var rid in request.RoleIds)
                {
                    var r = await _unitOfWork.Roles.GetByIdAsync(rid);
                    if (r?.RoleName == "Admin") willHaveAdmin = true;
                }

                if (user.UserRoles.Any(ur => ur.Role.RoleName == "Admin") && !willHaveAdmin && await _unitOfWork.Users.IsLastAdminAsync(userId))
                {
                    return ApiResponse<UserResponseDto>.Fail("Cannot remove Admin role from the last admin.");
                }

                user.UserRoles.Clear();
                foreach (var roleId in request.RoleIds)
                {
                    user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId, AssignedAt = DateTime.Now });
                }
            }

            user.UpdatedAt = DateTime.Now;
            await _unitOfWork.SaveChangesAsync();

            var updatedUser = await _unitOfWork.Users.GetUserWithRolesAsync(userId);
            return ApiResponse<UserResponseDto>.Ok(MapToResponseDto(updatedUser!), "User updated successfully.");
        }

        public async Task<ApiResponse<bool>> LockUserAsync(int currentUserId, int userId)
        {
            var result = await UpdateUserAsync(currentUserId, userId, new UpdateUserRequestDto { IsActive = false });
            return new ApiResponse<bool> { Success = result.Success, Message = result.Message, Data = result.Success };
        }

        public async Task<ApiResponse<bool>> UnlockUserAsync(int currentUserId, int userId)
        {
            var result = await UpdateUserAsync(currentUserId, userId, new UpdateUserRequestDto { IsActive = true });
            return new ApiResponse<bool> { Success = result.Success, Message = result.Message, Data = result.Success };
        }

        public async Task<ApiResponse<bool>> AssignRolesAsync(int currentUserId, AssignRoleRequestDto request)
        {
            var result = await UpdateUserAsync(currentUserId, request.UserId, new UpdateUserRequestDto { RoleIds = request.RoleIds });
            return new ApiResponse<bool> { Success = result.Success, Message = result.Message, Data = result.Success };
        }

        public async Task<ApiResponse<List<RoleResponseDto>>> GetRolesAsync()
        {
            var roles = await _unitOfWork.Roles.GetAllAsync();
            var response = roles.Select(r => new RoleResponseDto
            {
                RoleId = r.RoleId, RoleName = r.RoleName, Description = r.Description, IsActive = r.IsActive
            }).ToList();
            return ApiResponse<List<RoleResponseDto>>.Ok(response);
        }

        private static UserResponseDto MapToResponseDto(User user)
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
                EmployeeId = user.Employee?.EmployeeId,
                FullName = user.Employee?.FullName,
                EmployeeCode = user.Employee?.EmployeeCode,
                Roles = user.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.RoleName).ToList()
            };
        }
    }
}