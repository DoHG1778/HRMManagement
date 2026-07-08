using HRM.Business.Common;
using HRM.Business.DTOs.Departments;

namespace HRM.Business.Services.Interfaces
{
    public interface IDepartmentService
    {
        Task<ApiResponse<List<DepartmentResponseDto>>> GetDepartmentsAsync(
            bool? isActive = null);

        Task<ApiResponse<DepartmentResponseDto>> GetDepartmentByIdAsync(
            int departmentId);

        Task<ApiResponse<DepartmentResponseDto>> CreateDepartmentAsync(
            CurrentUser currentUser,
            CreateDepartmentRequestDto request);

        Task<ApiResponse<DepartmentResponseDto>> UpdateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId,
            UpdateDepartmentRequestDto request);

        Task<ApiResponse<bool>> DeactivateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId);

        Task<ApiResponse<bool>> ActivateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId);
    }
}