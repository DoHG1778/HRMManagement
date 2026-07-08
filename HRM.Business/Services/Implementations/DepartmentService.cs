using HRM.Business.Common;
using HRM.Business.DTOs.Departments;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

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
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<DepartmentResponseDto>> GetDepartmentByIdAsync(
            int departmentId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<DepartmentResponseDto>> CreateDepartmentAsync(
            CurrentUser currentUser,
            CreateDepartmentRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<DepartmentResponseDto>> UpdateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId,
            UpdateDepartmentRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> DeactivateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> ActivateDepartmentAsync(
            CurrentUser currentUser,
            int departmentId)
        {
            throw new NotImplementedException();
        }
    }
}