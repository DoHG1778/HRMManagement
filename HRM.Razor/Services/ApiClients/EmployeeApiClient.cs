using HRM.Business.Common;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class EmployeeApiClient : IEmployeeApiClient
    {
        private readonly IApiClient _apiClient;

        public EmployeeApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<PagedResultModel<EmployeeItemModel>>> GetEmployeesAsync(
            string? keyword = null,
            int? departmentId = null,
            int? positionId = null,
            string? status = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(keyword))
                queryParams.Add($"keyword={Uri.EscapeDataString(keyword.Trim())}");

            if (departmentId.HasValue)
                queryParams.Add($"departmentId={departmentId.Value}");

            if (positionId.HasValue)
                queryParams.Add($"positionId={positionId.Value}");

            if (!string.IsNullOrWhiteSpace(status))
                queryParams.Add($"employmentStatus={Uri.EscapeDataString(status.Trim())}");

            var endpoint = $"api/employees?{string.Join("&", queryParams)}";
            return await _apiClient.GetAsync<PagedResultModel<EmployeeItemModel>>(endpoint);
        }

        public async Task<ApiResponse<EmployeeDetailModel>> GetEmployeeDetailAsync(int employeeId)
        {
            return await _apiClient.GetAsync<EmployeeDetailModel>($"api/employees/{employeeId}");
        }

        public async Task<ApiResponse<EmployeeItemModel>> CreateEmployeeAsync(CreateEmployeeViewModel model)
        {
            var requestData = new
            {
                EmployeeCode = model.EmployeeCode?.Trim() ?? string.Empty,
                FullName = model.FullName?.Trim() ?? string.Empty,
                Gender = model.Gender?.Trim() ?? "Nam",
                DateOfBirth = model.DateOfBirth,
                Phone = model.Phone?.Trim(),
                Email = model.Email?.Trim() ?? string.Empty,
                Address = model.Address?.Trim(),
                Cccd = model.Cccd?.Trim(),
                HireDate = model.HireDate,
                UserId = model.UserId,
                ManagerId = model.ManagerId,
                AvatarUrl = model.AvatarUrl?.Trim()
            };

            return await _apiClient.PostAsync<EmployeeItemModel>("api/employees", requestData);
        }

        public async Task<ApiResponse<EmployeeItemModel>> UpdateEmployeeAsync(int employeeId, EditEmployeeViewModel model)
        {
            var requestData = new
            {
                FullName = model.FullName?.Trim(),
                Gender = model.Gender?.Trim(),
                DateOfBirth = model.DateOfBirth,
                Phone = model.Phone?.Trim(),
                Email = model.Email?.Trim(),
                Address = model.Address?.Trim(),
                Cccd = model.Cccd?.Trim(),
                HireDate = model.HireDate,
                EmploymentStatus = model.EmploymentStatus?.Trim(),
                UserId = model.UserId,
                ManagerId = model.ManagerId,
                AvatarUrl = model.AvatarUrl?.Trim()
            };

            return await _apiClient.PutAsync<EmployeeItemModel>($"api/employees/{employeeId}", requestData);
        }

        public async Task<ApiResponse<bool>> ChangeEmployeeStatusAsync(int employeeId, string status)
        {
            var endpoint = $"api/employees/{employeeId}/status?status={Uri.EscapeDataString(status.Trim())}";
            return await _apiClient.PutAsync<bool>(endpoint, new { });
        }

        public async Task<ApiResponse<EmployeeDetailModel>> GetMyProfileAsync()
        {
            return await _apiClient.GetAsync<EmployeeDetailModel>("api/employees/me");
        }

        public async Task<ApiResponse<EmployeeDetailModel>> UpdateMyProfileAsync(UpdateMyProfileViewModel model)
        {
            var requestData = new
            {
                Phone = model.Phone?.Trim(),
                Address = model.Address?.Trim(),
                AvatarUrl = model.AvatarUrl?.Trim()
            };

            return await _apiClient.PutAsync<EmployeeDetailModel>("api/employees/me", requestData);
        }
    }
}
