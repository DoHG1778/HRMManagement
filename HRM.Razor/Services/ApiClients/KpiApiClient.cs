using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels.Kpis;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class KpiApiClient : IKpiApiClient
    {
        private readonly IApiClient _apiClient;

        public KpiApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<KpiViewModel>>>
            GetKpisAsync(bool? isActive = null)
        {
            var endpoint = "api/kpis";

            if (isActive.HasValue)
            {
                endpoint +=
                    $"?isActive={isActive.Value.ToString().ToLower()}";
            }

            return await _apiClient
                .GetAsync<List<KpiViewModel>>(endpoint);
        }

        public async Task<ApiResponse<KpiViewModel>>
            GetKpiByIdAsync(int kpiId)
        {
            return await _apiClient
                .GetAsync<KpiViewModel>(
                    $"api/kpis/{kpiId}");
        }

        public async Task<ApiResponse<KpiViewModel>>
            CreateKpiAsync(CreateKpiViewModel model)
        {
            var requestData = new
            {
                Kpiname = model.Kpiname.Trim(),
                Description = model.Description?.Trim(),
                Weight = model.Weight
            };

            return await _apiClient
                .PostAsync<KpiViewModel>(
                    "api/kpis",
                    requestData);
        }

        public async Task<ApiResponse<KpiViewModel>>
            UpdateKpiAsync(
                int kpiId,
                UpdateKpiViewModel model)
        {
            var requestData = new
            {
                Kpiname = model.Kpiname?.Trim(),
                Description = model.Description?.Trim(),
                Weight = model.Weight,
                IsActive = model.IsActive
            };

            return await _apiClient
                .PutAsync<KpiViewModel>(
                    $"api/kpis/{kpiId}",
                    requestData);
        }

        public async Task<ApiResponse<bool>>
            DeactivateKpiAsync(int kpiId)
        {
            return await _apiClient
                .PutAsync<bool>(
                    $"api/kpis/{kpiId}/deactivate",
                    new { });
        }

        public async Task<ApiResponse<KpiAssignmentViewModel>>
            AssignKpiAsync(
                AssignKpiViewModel model)
        {
            var requestData = new
            {
                Kpiid = model.Kpiid,
                EmployeeId = model.EmployeeId,
                TargetValue = model.TargetValue,
                StartDate = model.StartDate,
                EndDate = model.EndDate
            };

            return await _apiClient
                .PostAsync<KpiAssignmentViewModel>(
                    "api/kpis/assignments",
                    requestData);
        }

        public async Task<ApiResponse<KpiAssignmentViewModel>>
            UpdateKpiProgressAsync(
                int assignmentId,
                UpdateKpiProgressViewModel model)
        {
            var requestData = new
            {
                ActualValue = model.ActualValue,
                ProgressPercent = model.ProgressPercent,
                EmployeeComment = model.EmployeeComment?.Trim(),
                EmployeeSelfScore = model.EmployeeSelfScore
            };

            return await _apiClient
                .PutAsync<KpiAssignmentViewModel>(
                    $"api/kpis/assignments/{assignmentId}/progress",
                    requestData);
        }

        public async Task<ApiResponse<KpiAssignmentViewModel>>
            EvaluateKpiAsync(
                int assignmentId,
                EvaluateKpiViewModel model)
        {
            var requestData = new
            {
                ManagerScore = model.ManagerScore,
                ManagerComment = model.ManagerComment?.Trim()
            };

            return await _apiClient
                .PutAsync<KpiAssignmentViewModel>(
                    $"api/kpis/assignments/{assignmentId}/evaluate",
                    requestData);
        }

        public async Task<ApiResponse<
            HRM.Razor.Models.PagedResultModel<KpiAssignmentViewModel>>>
            GetMyKpiAssignmentsAsync(
                int? kpiId = null,
                string? status = null,
                DateOnly? fromDate = null,
                DateOnly? toDate = null,
                int pageNumber = 1,
                int pageSize = 10)
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (kpiId.HasValue)
                queryParams.Add($"kpiid={kpiId.Value}");

            if (!string.IsNullOrWhiteSpace(status))
            {
                queryParams.Add(
                    $"status={Uri.EscapeDataString(status.Trim())}");
            }

            if (fromDate.HasValue)
                queryParams.Add(
                    $"fromDate={fromDate.Value:yyyy-MM-dd}");

            if (toDate.HasValue)
                queryParams.Add(
                    $"toDate={toDate.Value:yyyy-MM-dd}");

            var endpoint =
                $"api/kpis/assignments/me?{string.Join("&", queryParams)}";

            return await _apiClient.GetAsync<
                HRM.Razor.Models.PagedResultModel<KpiAssignmentViewModel>
            >(endpoint);
        }

        public async Task<ApiResponse<
            HRM.Razor.Models.PagedResultModel<KpiAssignmentViewModel>>>
            GetKpiAssignmentsAsync(
                int? employeeId = null,
                int? kpiId = null,
                string? status = null,
                DateOnly? fromDate = null,
                DateOnly? toDate = null,
                int pageNumber = 1,
                int pageSize = 10)
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (employeeId.HasValue)
                queryParams.Add(
                    $"employeeId={employeeId.Value}");

            if (kpiId.HasValue)
                queryParams.Add(
                    $"kpiid={kpiId.Value}");

            if (!string.IsNullOrWhiteSpace(status))
            {
                queryParams.Add(
                    $"status={Uri.EscapeDataString(status.Trim())}");
            }

            if (fromDate.HasValue)
                queryParams.Add(
                    $"fromDate={fromDate.Value:yyyy-MM-dd}");

            if (toDate.HasValue)
                queryParams.Add(
                    $"toDate={toDate.Value:yyyy-MM-dd}");

            var endpoint =
                $"api/kpis/assignments?{string.Join("&", queryParams)}";

            return await _apiClient.GetAsync<
                HRM.Razor.Models.PagedResultModel<KpiAssignmentViewModel>
            >(endpoint);
        }
    }
}