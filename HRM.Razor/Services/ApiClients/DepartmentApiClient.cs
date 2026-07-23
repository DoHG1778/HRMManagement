using HRM.Business.Common;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class DepartmentApiClient : IDepartmentApiClient
    {
        private readonly IApiClient _apiClient;

        public DepartmentApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<DepartmentModel>>> GetDepartmentsAsync(bool? isActive = null)
        {
            var endpoint = isActive.HasValue
                ? $"api/departments?isActive={isActive.Value.ToString().ToLower()}"
                : "api/departments";
            return await _apiClient.GetAsync<List<DepartmentModel>>(endpoint);
        }

        public async Task<ApiResponse<DepartmentModel>> GetDepartmentByIdAsync(int departmentId)
        {
            return await _apiClient.GetAsync<DepartmentModel>($"api/departments/{departmentId}");
        }

        public async Task<ApiResponse<DepartmentModel>> CreateDepartmentAsync(CreateDepartmentViewModel model)
        {
            var requestData = new
            {
                DepartmentName = model.DepartmentName?.Trim() ?? string.Empty,
                Description = model.Description?.Trim(),
                ManagerEmployeeId = model.ManagerEmployeeId
            };
            return await _apiClient.PostAsync<DepartmentModel>("api/departments", requestData);
        }

        public async Task<ApiResponse<DepartmentModel>> UpdateDepartmentAsync(int departmentId, EditDepartmentViewModel model)
        {
            var requestData = new
            {
                DepartmentName = model.DepartmentName?.Trim(),
                Description = model.Description?.Trim(),
                ManagerEmployeeId = model.ManagerEmployeeId,
                IsActive = model.IsActive
            };
            return await _apiClient.PutAsync<DepartmentModel>($"api/departments/{departmentId}", requestData);
        }

        public async Task<ApiResponse<bool>> DeactivateDepartmentAsync(int departmentId)
        {
            return await _apiClient.PutAsync<bool>($"api/departments/{departmentId}/deactivate", new { });
        }

        public async Task<ApiResponse<bool>> ActivateDepartmentAsync(int departmentId)
        {
            return await _apiClient.PutAsync<bool>($"api/departments/{departmentId}/activate", new { });
        }
    }
}
