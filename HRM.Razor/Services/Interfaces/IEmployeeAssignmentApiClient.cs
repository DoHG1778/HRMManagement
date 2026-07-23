using HRM.Business.Common;
using HRM.Razor.Models.ViewModels.EmployeeAssignments;

namespace HRM.Razor.Services.Interfaces
{
    public interface IEmployeeAssignmentApiClient
    {
        Task<ApiResponse<EmployeeAssignmentItemViewModel>> AssignEmployeeAsync(int employeeId, AssignEmployeeViewModel model);

        Task<ApiResponse<EmployeeAssignmentItemViewModel>> TransferEmployeeAsync(int employeeId, TransferEmployeeViewModel model);

        Task<ApiResponse<List<EmployeeAssignmentItemViewModel>>> GetAssignmentHistoryAsync(int employeeId);
    }
}
