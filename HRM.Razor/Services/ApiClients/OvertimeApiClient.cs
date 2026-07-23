using HRM.Business.Common;
using HRM.Business.DTOs.Overtimes;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class OvertimeApiClient : IOvertimeApiClient
    {
        private readonly IApiClient _apiClient;

        public OvertimeApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<PagedResult<OvertimeRequestResponseDto>>> GetMyOvertimeRequestsAsync(
            int? month = null, int? year = null, string? status = null,
            int pageNumber = 1, int pageSize = 50)
        {
            var endpoint = $"api/overtimes/requests/me?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(status)) endpoint += $"&status={status}";
            if (month.HasValue) endpoint += $"&month={month}";
            if (year.HasValue) endpoint += $"&year={year}";
            return await _apiClient.GetAsync<PagedResult<OvertimeRequestResponseDto>>(endpoint);
        }

        public async Task<ApiResponse<List<OvertimeRequestResponseDto>>> GetPendingOvertimeRequestsAsync()
        {
            return await _apiClient.GetAsync<List<OvertimeRequestResponseDto>>("api/overtimes/requests/pending");
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> CreateOvertimeRequestAsync(
            CreateOvertimeRequestDto model)
        {
            return await _apiClient.PostAsync<OvertimeRequestResponseDto>("api/overtimes/requests", model);
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> UpdateOvertimeRequestAsync(
            int overtimeRequestId, UpdateOvertimeRequestDto model)
        {
            return await _apiClient.PutAsync<OvertimeRequestResponseDto>(
                $"api/overtimes/requests/{overtimeRequestId}", model);
        }

        public async Task<ApiResponse<bool>> CancelOvertimeRequestAsync(int overtimeRequestId)
        {
            return await _apiClient.PutAsync<bool>(
                $"api/overtimes/requests/{overtimeRequestId}/cancel", new { });
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> ApproveOvertimeRequestAsync(
            int overtimeRequestId, ApproveOvertimeRequestDto model)
        {
            return await _apiClient.PutAsync<OvertimeRequestResponseDto>(
                $"api/overtimes/requests/{overtimeRequestId}/approval", model);
        }
    }
}
