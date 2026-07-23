using HRM.Business.Common;
using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class LeaveApiClient : ILeaveApiClient
    {
        private readonly IApiClient _apiClient;

        public LeaveApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<PagedResult<LeaveRequestResponseDto>>> GetMyLeaveRequestsAsync(
            int? month = null, int? year = null, string? status = null,
            int pageNumber = 1, int pageSize = 50)
        {
            var endpoint = $"api/leaves/requests/me?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(status)) endpoint += $"&status={status}";
            if (month.HasValue) endpoint += $"&month={month}";
            if (year.HasValue) endpoint += $"&year={year}";
            return await _apiClient.GetAsync<PagedResult<LeaveRequestResponseDto>>(endpoint);
        }

        public async Task<ApiResponse<List<LeaveRequestResponseDto>>> GetPendingLeaveRequestsAsync()
        {
            return await _apiClient.GetAsync<List<LeaveRequestResponseDto>>("api/leaves/requests/pending");
        }

        public async Task<ApiResponse<List<LeaveTypeResponseDto>>> GetLeaveTypesAsync(bool? isActive = null)
        {
            var endpoint = isActive.HasValue
                ? $"api/leaves/types?isActive={isActive.Value.ToString().ToLower()}"
                : "api/leaves/types";
            return await _apiClient.GetAsync<List<LeaveTypeResponseDto>>(endpoint);
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> CreateLeaveRequestAsync(CreateLeaveRequestDto model)
        {
            return await _apiClient.PostAsync<LeaveRequestResponseDto>("api/leaves/requests", model);
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> UpdateLeaveRequestAsync(
            int leaveRequestId, UpdateLeaveRequestDto model)
        {
            return await _apiClient.PutAsync<LeaveRequestResponseDto>(
                $"api/leaves/requests/{leaveRequestId}", model);
        }

        public async Task<ApiResponse<bool>> CancelLeaveRequestAsync(int leaveRequestId)
        {
            return await _apiClient.PutAsync<bool>(
                $"api/leaves/requests/{leaveRequestId}/cancel", new { });
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> ApproveLeaveRequestAsync(
            int leaveRequestId, ApproveLeaveRequestDto model)
        {
            return await _apiClient.PutAsync<LeaveRequestResponseDto>(
                $"api/leaves/requests/{leaveRequestId}/approval", model);
        }

        public async Task<ApiResponse<LeaveTypeResponseDto>> CreateLeaveTypeAsync(CreateLeaveTypeRequestDto model)
        {
            return await _apiClient.PostAsync<LeaveTypeResponseDto>("api/leaves/types", model);
        }

        public async Task<ApiResponse<LeaveTypeResponseDto>> UpdateLeaveTypeAsync(
            int leaveTypeId, UpdateLeaveTypeRequestDto model)
        {
            return await _apiClient.PutAsync<LeaveTypeResponseDto>(
                $"api/leaves/types/{leaveTypeId}", model);
        }

        public async Task<ApiResponse<bool>> DeactivateLeaveTypeAsync(int leaveTypeId)
        {
            return await _apiClient.PutAsync<bool>(
                $"api/leaves/types/{leaveTypeId}/deactivate", new { });
        }

        public async Task<ApiResponse<bool>> ActivateLeaveTypeAsync(int leaveTypeId)
        {
            return await _apiClient.PutAsync<bool>(
                $"api/leaves/types/{leaveTypeId}/activate", new { });
        }

        public async Task<ApiResponse<List<LeaveBalanceResponseDto>>> GetLeaveBalancesAsync(
            int employeeId, int? year = null)
        {
            var endpoint = $"api/leaves/balances/employee/{employeeId}";
            if (year.HasValue) endpoint += $"?year={year}";
            return await _apiClient.GetAsync<List<LeaveBalanceResponseDto>>(endpoint);
        }

        public async Task<ApiResponse<LeaveBalanceResponseDto>> SetLeaveBalanceAsync(SetLeaveBalanceRequestDto model)
        {
            return await _apiClient.PostAsync<LeaveBalanceResponseDto>("api/leaves/balances", model);
        }
    }
}
