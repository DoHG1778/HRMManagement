using HRM.Business.Common;
using HRM.Business.DTOs.Overtimes;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.Interfaces
{
    public interface IOvertimeApiClient
    {
        Task<ApiResponse<PagedResult<OvertimeRequestResponseDto>>> GetMyOvertimeRequestsAsync(
            int? month = null, int? year = null, string? status = null,
            int pageNumber = 1, int pageSize = 50);

        Task<ApiResponse<List<OvertimeRequestResponseDto>>> GetPendingOvertimeRequestsAsync();

        Task<ApiResponse<OvertimeRequestResponseDto>> CreateOvertimeRequestAsync(CreateOvertimeRequestDto model);

        Task<ApiResponse<OvertimeRequestResponseDto>> UpdateOvertimeRequestAsync(
            int overtimeRequestId, UpdateOvertimeRequestDto model);

        Task<ApiResponse<bool>> CancelOvertimeRequestAsync(int overtimeRequestId);

        Task<ApiResponse<OvertimeRequestResponseDto>> ApproveOvertimeRequestAsync(
            int overtimeRequestId, ApproveOvertimeRequestDto model);
    }
}
