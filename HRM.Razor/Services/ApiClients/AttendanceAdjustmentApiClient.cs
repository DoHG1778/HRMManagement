using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels.AttendanceAdjustments;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class AttendanceAdjustmentApiClient : IAttendanceAdjustmentApiClient
    {
        private readonly IApiClient _apiClient;

        public AttendanceAdjustmentApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<AttendanceAdjustmentItemModel>>> GetMyAdjustmentsAsync(
            string? status = null,
            int? month = null,
            int? year = null)
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(status)) queryParams.Add($"status={Uri.EscapeDataString(status.Trim())}");
            if (month.HasValue && month.Value > 0) queryParams.Add($"month={month.Value}");
            if (year.HasValue && year.Value > 0) queryParams.Add($"year={year.Value}");

            var queryString = string.Join("&", queryParams);
            var endpoint = string.IsNullOrEmpty(queryString) ? "api/attendance-adjustments/my" : $"api/attendance-adjustments/my?{queryString}";

            return await _apiClient.GetAsync<List<AttendanceAdjustmentItemModel>>(endpoint);
        }

        public async Task<ApiResponse<List<AdjustableAttendanceModel>>> GetAdjustableAttendancesAsync(
            int? month = null,
            int? year = null)
        {
            var queryParams = new List<string>();
            if (month.HasValue && month.Value > 0) queryParams.Add($"month={month.Value}");
            if (year.HasValue && year.Value > 0) queryParams.Add($"year={year.Value}");

            var queryString = string.Join("&", queryParams);
            var endpoint = string.IsNullOrEmpty(queryString) ? "api/attendance-adjustments/available-attendances" : $"api/attendance-adjustments/available-attendances?{queryString}";

            return await _apiClient.GetAsync<List<AdjustableAttendanceModel>>(endpoint);
        }

        public async Task<ApiResponse<AttendanceAdjustmentItemModel>> GetAdjustmentByIdAsync(int id)
        {
            return await _apiClient.GetAsync<AttendanceAdjustmentItemModel>($"api/attendance-adjustments/{id}");
        }

        public async Task<ApiResponse<AttendanceAdjustmentItemModel>> CreateAdjustmentAsync(CreateAttendanceAdjustmentViewModel model)
        {
            var requestData = new
            {
                AttendanceId = model.AttendanceId,
                RequestedCheckInTime = model.RequestedCheckInTime,
                RequestedCheckOutTime = model.RequestedCheckOutTime,
                Reason = model.Reason?.Trim() ?? string.Empty
            };
            return await _apiClient.PostAsync<AttendanceAdjustmentItemModel>("api/attendance-adjustments", requestData);
        }

        public async Task<ApiResponse<AttendanceAdjustmentItemModel>> UpdateAdjustmentAsync(int id, UpdateAttendanceAdjustmentViewModel model)
        {
            var requestData = new
            {
                RequestedCheckInTime = model.RequestedCheckInTime,
                RequestedCheckOutTime = model.RequestedCheckOutTime,
                Reason = model.Reason?.Trim() ?? string.Empty
            };
            return await _apiClient.PutAsync<AttendanceAdjustmentItemModel>($"api/attendance-adjustments/{id}", requestData);
        }

        public async Task<ApiResponse<bool>> CancelAdjustmentAsync(int id)
        {
            return await _apiClient.PutAsync<bool>($"api/attendance-adjustments/{id}/cancel", new { });
        }

        public async Task<ApiResponse<List<AttendanceAdjustmentItemModel>>> GetPendingAdjustmentsAsync(
            int? employeeId = null,
            int? month = null,
            int? year = null)
        {
            var queryParams = new List<string>();
            if (employeeId.HasValue && employeeId.Value > 0) queryParams.Add($"employeeId={employeeId.Value}");
            if (month.HasValue && month.Value > 0) queryParams.Add($"month={month.Value}");
            if (year.HasValue && year.Value > 0) queryParams.Add($"year={year.Value}");

            var queryString = string.Join("&", queryParams);
            var endpoint = string.IsNullOrEmpty(queryString) ? "api/attendance-adjustments/pending" : $"api/attendance-adjustments/pending?{queryString}";

            return await _apiClient.GetAsync<List<AttendanceAdjustmentItemModel>>(endpoint);
        }

        public async Task<ApiResponse<AttendanceAdjustmentItemModel>> ApproveAdjustmentAsync(int id)
        {
            return await _apiClient.PutAsync<AttendanceAdjustmentItemModel>($"api/attendance-adjustments/{id}/approve", new { });
        }

        public async Task<ApiResponse<AttendanceAdjustmentItemModel>> RejectAdjustmentAsync(int id, string rejectionReason)
        {
            var requestData = new
            {
                RejectionReason = rejectionReason?.Trim() ?? string.Empty
            };
            return await _apiClient.PutAsync<AttendanceAdjustmentItemModel>($"api/attendance-adjustments/{id}/reject", requestData);
        }
    }
}
