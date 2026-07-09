using HRM.Business.Common;
using HRM.Business.DTOs.Departments;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<DepartmentResponseDto>>> GetDepartmentsAsync(
            bool? isActive = null)
        {
            var query = _unitOfWork.Departments.Query()
                .Include(d => d.ManagerEmployee)
                .Include(d => d.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(d => d.IsActive == isActive.Value);
            }

            var departments = await query
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            return ApiResponse<List<DepartmentResponseDto>>.Ok(
                departments.Select(MapDepartmentToDto).ToList());
        }

        public async Task<ApiResponse<DepartmentResponseDto>> GetDepartmentByIdAsync(
            int departmentId)
        {
            var department = await _unitOfWork.Departments.GetDepartmentDetailAsync(departmentId);
            if (department == null)
                return ApiResponse<DepartmentResponseDto>.NotFound("Department not found.");

            return ApiResponse<DepartmentResponseDto>.Ok(MapDepartmentToDto(department));
        }

        public async Task<ApiResponse<DepartmentResponseDto>> CreateDepartmentAsync(
            CurrentUser currentUser,
            CreateDepartmentRequestDto request)
        {
            if (!CanManageOrganization(currentUser))
                return ApiResponse<DepartmentResponseDto>.Forbidden("You are not allowed to manage departments.");

            if (request == null)
                return ApiResponse<DepartmentResponseDto>.Fail("Department information is required.");

            var departmentName = request.DepartmentName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(departmentName))
                return ApiResponse<DepartmentResponseDto>.Fail("Department name is required.");

            var nameExists = await _unitOfWork.Departments.Query()
                .AnyAsync(d => d.DepartmentName == departmentName);
            if (nameExists)
                return ApiResponse<DepartmentResponseDto>.Fail("Department name already exists.");

            if (request.ManagerEmployeeId.HasValue)
            {
                var managerValid = await _unitOfWork.Employees.IsActiveEmployeeAsync(request.ManagerEmployeeId.Value);
                if (!managerValid)
                    return ApiResponse<DepartmentResponseDto>.Fail("Manager must be an active employee.");
            }

            var department = new Department
            {
                DepartmentName = departmentName,
                Description = request.Description,
                ManagerEmployeeId = request.ManagerEmployeeId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Departments.GetDepartmentDetailAsync(department.DepartmentId);
            return ApiResponse<DepartmentResponseDto>.Ok(
                MapDepartmentToDto(created ?? department),
                "Department created successfully.",
                201);
        }

        public async Task<ApiResponse<DepartmentResponseDto>> UpdateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId,
            UpdateDepartmentRequestDto request)
        {
            if (!CanManageOrganization(currentUser))
                return ApiResponse<DepartmentResponseDto>.Forbidden("You are not allowed to manage departments.");

            if (request == null)
                return ApiResponse<DepartmentResponseDto>.Fail("Department information is required.");

            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department == null)
                return ApiResponse<DepartmentResponseDto>.NotFound("Department not found.");

            if (!string.IsNullOrWhiteSpace(request.DepartmentName))
            {
                var departmentName = request.DepartmentName.Trim();
                var nameExists = await _unitOfWork.Departments.Query()
                    .AnyAsync(d =>
                        d.DepartmentId != departmentId &&
                        d.DepartmentName == departmentName);
                if (nameExists)
                    return ApiResponse<DepartmentResponseDto>.Fail("Department name already exists.");

                department.DepartmentName = departmentName;
            }

            if (request.Description != null)
            {
                department.Description = request.Description;
            }

            if (request.ManagerEmployeeId.HasValue)
            {
                var managerValid = await _unitOfWork.Employees.IsActiveEmployeeAsync(request.ManagerEmployeeId.Value);
                if (!managerValid)
                    return ApiResponse<DepartmentResponseDto>.Fail("Manager must be an active employee.");

                department.ManagerEmployeeId = request.ManagerEmployeeId.Value;
            }

            if (request.IsActive.HasValue)
            {
                if (!request.IsActive.Value && await _unitOfWork.Departments.HasActiveEmployeesAsync(departmentId))
                    return ApiResponse<DepartmentResponseDto>.Fail("Cannot deactivate department while it still has active employees.");

                department.IsActive = request.IsActive.Value;
            }

            department.UpdatedAt = DateTime.Now;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Departments.GetDepartmentDetailAsync(departmentId);
            return ApiResponse<DepartmentResponseDto>.Ok(
                MapDepartmentToDto(updated ?? department),
                "Department updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeactivateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId)
        {
            if (!CanManageOrganization(currentUser))
                return ApiResponse<bool>.Forbidden("You are not allowed to manage departments.");

            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department == null)
                return ApiResponse<bool>.NotFound("Department not found.");

            if (await _unitOfWork.Departments.HasActiveEmployeesAsync(departmentId))
                return ApiResponse<bool>.Fail("Cannot deactivate department while it still has active employees.");

            department.IsActive = false;
            department.UpdatedAt = DateTime.Now;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Department deactivated successfully.");
        }

        public async Task<ApiResponse<bool>> ActivateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId)
        {
            if (!CanManageOrganization(currentUser))
                return ApiResponse<bool>.Forbidden("You are not allowed to manage departments.");

            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department == null)
                return ApiResponse<bool>.NotFound("Department not found.");

            department.IsActive = true;
            department.UpdatedAt = DateTime.Now;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Department activated successfully.");
        }

        private static bool CanManageOrganization(CurrentUser currentUser)
        {
            return currentUser.HasAnyRole("Admin", "HR", "HR Staff", "System Administrator");
        }

        private static DepartmentResponseDto MapDepartmentToDto(Department department)
        {
            return new DepartmentResponseDto
            {
                DepartmentId = department.DepartmentId,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                ManagerEmployeeId = department.ManagerEmployeeId,
                ManagerName = department.ManagerEmployee?.FullName,
                IsActive = department.IsActive,
                EmployeeCount = department.EmployeeAssignments.Count(ea =>
                    ea.EndDate == null &&
                    ea.Employee != null &&
                    ea.Employee.EmploymentStatus == "ACTIVE"),
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };
        }
    }
}
