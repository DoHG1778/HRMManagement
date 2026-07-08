using HRM.Business.Common;
using HRM.Business.DTOs.Employees;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PagedResult<EmployeeResponseDto>>> GetEmployeesAsync(
            CurrentUser currentUser,
            EmployeeFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<EmployeeDetailResponseDto>> GetEmployeeDetailAsync(
            CurrentUser currentUser,
            int employeeId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<EmployeeResponseDto>> CreateEmployeeAsync(
            CurrentUser currentUser,
            CreateEmployeeRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<EmployeeResponseDto>> UpdateEmployeeAsync(
            CurrentUser currentUser,
            int employeeId,
            UpdateEmployeeRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<EmployeeDetailResponseDto>> GetMyProfileAsync(
            CurrentUser currentUser)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<EmployeeDetailResponseDto>> UpdateMyProfileAsync(
            CurrentUser currentUser,
            UpdateMyProfileRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> ChangeEmployeeStatusAsync(
            CurrentUser currentUser,
            int employeeId,
            string status)
        {
            throw new NotImplementedException();
        }
    }
}