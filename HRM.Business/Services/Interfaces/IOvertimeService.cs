using HRM.Business.Common;
using HRM.Business.DTOs.Overtimes;

namespace HRM.Business.Services.Interfaces
{
    public interface IOvertimeService
    {
        Task<ApiResponse<OvertimeRequestResponseDto>> CreateOvertimeRequestAsync(
            CurrentUser currentUser,
            CreateOvertimeRequestDto request);

        Task<ApiResponse<OvertimeRequestResponseDto>> UpdateOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId,
            UpdateOvertimeRequestDto request);

        Task<ApiResponse<bool>> CancelOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId);

        Task<ApiResponse<PagedResult<OvertimeRequestResponseDto>>> GetMyOvertimeRequestsAsync(
            CurrentUser currentUser,
            OvertimeRequestFilterDto filter);

        Task<ApiResponse<PagedResult<OvertimeRequestResponseDto>>> GetOvertimeRequestsAsync(
            CurrentUser currentUser,
            OvertimeRequestFilterDto filter);

        Task<ApiResponse<List<OvertimeRequestResponseDto>>> GetPendingOvertimeRequestsAsync(
            CurrentUser currentUser);

        Task<ApiResponse<OvertimeRequestResponseDto>> ApproveOrRejectOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId,
            ApproveOvertimeRequestDto request);

        Task<ApiResponse<List<OvertimeRequestResponseDto>>> GetApprovedOvertimeForPayrollAsync(
            CurrentUser currentUser,
            int employeeId,
            int month,
            int year);
    }
}