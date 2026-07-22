using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels;

namespace HRM.Razor.Services.Interfaces
{
    public interface IEmployeeApiClient
    {
        Task<ApiResponse<
            HRM.Razor.Models.PagedResultModel<EmployeeItemModel>>>
            GetEmployeesAsync(
                string? keyword = null,
                int? departmentId = null,
                int? positionId = null,
                string? status = null,
                int pageNumber = 1,
                int pageSize = 10);

        Task<ApiResponse<EmployeeDetailModel>> GetEmployeeDetailAsync(int employeeId);

        Task<ApiResponse<EmployeeItemModel>> CreateEmployeeAsync(CreateEmployeeViewModel model);

        Task<ApiResponse<EmployeeItemModel>> UpdateEmployeeAsync(int employeeId, EditEmployeeViewModel model);

        Task<ApiResponse<bool>> ChangeEmployeeStatusAsync(int employeeId, string status);

        Task<ApiResponse<EmployeeDetailModel>> GetMyProfileAsync();

        Task<ApiResponse<EmployeeDetailModel>> UpdateMyProfileAsync(UpdateMyProfileViewModel model);
    }
}
