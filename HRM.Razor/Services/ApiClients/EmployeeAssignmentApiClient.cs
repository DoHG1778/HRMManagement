using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels.EmployeeAssignments;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class EmployeeAssignmentApiClient : IEmployeeAssignmentApiClient
    {
        private readonly IApiClient _apiClient;

        public EmployeeAssignmentApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<EmployeeAssignmentItemViewModel>> AssignEmployeeAsync(int employeeId, AssignEmployeeViewModel model)
        {
            var requestData = new
            {
                DepartmentId = model.DepartmentId,
                PositionId = model.PositionId,
                StartDate = model.StartDate.ToString("yyyy-MM-dd"),
                Note = model.Note?.Trim()
            };
            return await _apiClient.PostAsync<EmployeeAssignmentItemViewModel>($"api/employees/{employeeId}/assignment", requestData);
        }

        public async Task<ApiResponse<EmployeeAssignmentItemViewModel>> TransferEmployeeAsync(int employeeId, TransferEmployeeViewModel model)
        {
            var requestData = new
            {
                DepartmentId = model.DepartmentId,
                PositionId = model.PositionId,
                StartDate = model.StartDate.ToString("yyyy-MM-dd"),
                Note = model.Note?.Trim()
            };
            return await _apiClient.PostAsync<EmployeeAssignmentItemViewModel>($"api/employees/{employeeId}/transfer", requestData);
        }

        public async Task<ApiResponse<List<EmployeeAssignmentItemViewModel>>> GetAssignmentHistoryAsync(int employeeId)
        {
            return await _apiClient.GetAsync<List<EmployeeAssignmentItemViewModel>>($"api/employees/{employeeId}/assignments");
        }
    }
}
