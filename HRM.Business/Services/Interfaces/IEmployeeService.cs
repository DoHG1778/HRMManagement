using HRM.Business.Common;
using HRM.Business.DTOs.Employees;

namespace HRM.Business.Services.Interfaces
{
    public interface IEmployeeService
    {
        Task<ApiResponse<PagedResult<EmployeeResponseDto>>> GetEmployeesAsync(
            CurrentUser currentUser,
            EmployeeFilterDto filter);

        Task<ApiResponse<EmployeeDetailResponseDto>> GetEmployeeDetailAsync(
            CurrentUser currentUser,
            int employeeId);

        Task<ApiResponse<EmployeeResponseDto>> CreateEmployeeAsync(
            CurrentUser currentUser,
            CreateEmployeeRequestDto request);

        Task<ApiResponse<EmployeeResponseDto>> UpdateEmployeeAsync(
            CurrentUser currentUser,
            int employeeId,
            UpdateEmployeeRequestDto request);

        Task<ApiResponse<EmployeeDetailResponseDto>> GetMyProfileAsync(
            CurrentUser currentUser);

        Task<ApiResponse<EmployeeDetailResponseDto>> UpdateMyProfileAsync(
            CurrentUser currentUser,
            UpdateMyProfileRequestDto request);

        Task<ApiResponse<bool>> ChangeEmployeeStatusAsync(
            CurrentUser currentUser,
            int employeeId,
            string status);
    }
}