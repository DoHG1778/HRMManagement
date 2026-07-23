using HRM.Business.Common;
using HRM.Razor.Models.ViewModels;

namespace HRM.Razor.Services.Interfaces
{
    public interface IDepartmentApiClient
    {
        Task<ApiResponse<List<DepartmentModel>>> GetDepartmentsAsync(bool? isActive = null);

        Task<ApiResponse<DepartmentModel>> GetDepartmentByIdAsync(int departmentId);

        Task<ApiResponse<DepartmentModel>> CreateDepartmentAsync(CreateDepartmentViewModel model);

        Task<ApiResponse<DepartmentModel>> UpdateDepartmentAsync(int departmentId, EditDepartmentViewModel model);

        Task<ApiResponse<bool>> DeactivateDepartmentAsync(int departmentId);

        Task<ApiResponse<bool>> ActivateDepartmentAsync(int departmentId);
    }
}
