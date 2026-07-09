using HRM.Business.Common;
using HRM.Business.DTOs.Positions;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class PositionService : IPositionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PositionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<PositionResponseDto>>> GetPositionsAsync(
            bool? isActive = null)
        {
            var query = _unitOfWork.Positions.Query()
                .Include(p => p.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var positions = await query
                .OrderBy(p => p.PositionName)
                .ToListAsync();

            return ApiResponse<List<PositionResponseDto>>.Ok(
                positions.Select(MapPositionToDto).ToList());
        }

        public async Task<ApiResponse<PositionResponseDto>> GetPositionByIdAsync(
            int positionId)
        {
            var position = await _unitOfWork.Positions.Query()
                .Include(p => p.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
                .FirstOrDefaultAsync(p => p.PositionId == positionId);

            if (position == null)
                return ApiResponse<PositionResponseDto>.NotFound("Position not found.");

            return ApiResponse<PositionResponseDto>.Ok(MapPositionToDto(position));
        }

        public async Task<ApiResponse<PositionResponseDto>> CreatePositionAsync(
            CurrentUser currentUser,
            CreatePositionRequestDto request)
        {
            if (!CanManageOrganization(currentUser))
                return ApiResponse<PositionResponseDto>.Forbidden("You are not allowed to manage positions.");

            if (request == null)
                return ApiResponse<PositionResponseDto>.Fail("Position information is required.");

            var positionName = request.PositionName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(positionName))
                return ApiResponse<PositionResponseDto>.Fail("Position name is required.");

            var nameExists = await _unitOfWork.Positions.Query()
                .AnyAsync(p => p.PositionName == positionName);
            if (nameExists)
                return ApiResponse<PositionResponseDto>.Fail("Position name already exists.");

            var position = new Position
            {
                PositionName = positionName,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Positions.AddAsync(position);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<PositionResponseDto>.Ok(
                MapPositionToDto(position),
                "Position created successfully.",
                201);
        }

        public async Task<ApiResponse<PositionResponseDto>> UpdatePositionAsync(
            CurrentUser currentUser,
            int positionId,
            UpdatePositionRequestDto request)
        {
            if (!CanManageOrganization(currentUser))
                return ApiResponse<PositionResponseDto>.Forbidden("You are not allowed to manage positions.");

            if (request == null)
                return ApiResponse<PositionResponseDto>.Fail("Position information is required.");

            var position = await _unitOfWork.Positions.GetByIdAsync(positionId);
            if (position == null)
                return ApiResponse<PositionResponseDto>.NotFound("Position not found.");

            if (!string.IsNullOrWhiteSpace(request.PositionName))
            {
                var positionName = request.PositionName.Trim();
                var nameExists = await _unitOfWork.Positions.Query()
                    .AnyAsync(p =>
                        p.PositionId != positionId &&
                        p.PositionName == positionName);
                if (nameExists)
                    return ApiResponse<PositionResponseDto>.Fail("Position name already exists.");

                position.PositionName = positionName;
            }

            if (request.Description != null)
            {
                position.Description = request.Description;
            }

            if (request.IsActive.HasValue)
            {
                position.IsActive = request.IsActive.Value;
            }

            position.UpdatedAt = DateTime.Now;
            _unitOfWork.Positions.Update(position);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Positions.Query()
                .Include(p => p.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
                .FirstAsync(p => p.PositionId == positionId);

            return ApiResponse<PositionResponseDto>.Ok(
                MapPositionToDto(updated),
                "Position updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeactivatePositionAsync(
            CurrentUser currentUser,
            int positionId)
        {
            if (!CanManageOrganization(currentUser))
                return ApiResponse<bool>.Forbidden("You are not allowed to manage positions.");

            var position = await _unitOfWork.Positions.GetByIdAsync(positionId);
            if (position == null)
                return ApiResponse<bool>.NotFound("Position not found.");

            position.IsActive = false;
            position.UpdatedAt = DateTime.Now;
            _unitOfWork.Positions.Update(position);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Position deactivated successfully.");
        }

        public async Task<ApiResponse<bool>> ActivatePositionAsync(
            CurrentUser currentUser,
            int positionId)
        {
            if (!CanManageOrganization(currentUser))
                return ApiResponse<bool>.Forbidden("You are not allowed to manage positions.");

            var position = await _unitOfWork.Positions.GetByIdAsync(positionId);
            if (position == null)
                return ApiResponse<bool>.NotFound("Position not found.");

            position.IsActive = true;
            position.UpdatedAt = DateTime.Now;
            _unitOfWork.Positions.Update(position);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Position activated successfully.");
        }

        private static bool CanManageOrganization(CurrentUser currentUser)
        {
            return currentUser.HasAnyRole("Admin", "HR", "HR Staff", "System Administrator");
        }

        private static PositionResponseDto MapPositionToDto(Position position)
        {
            return new PositionResponseDto
            {
                PositionId = position.PositionId,
                PositionName = position.PositionName,
                Description = position.Description,
                IsActive = position.IsActive,
                EmployeeCount = position.EmployeeAssignments.Count(ea =>
                    ea.EndDate == null &&
                    ea.Employee != null &&
                    ea.Employee.EmploymentStatus == "ACTIVE"),
                CreatedAt = position.CreatedAt,
                UpdatedAt = position.UpdatedAt
            };
        }
    }
}
