using HRM.Business.Common;
using HRM.Business.DTOs.Leaves;

namespace HRM.Business.Services.Interfaces
{
    public interface ILeaveService
    {
        Task<ApiResponse<LeaveRequestResponseDto>> CreateLeaveRequestAsync(
            CurrentUser currentUser,
            CreateLeaveRequestDto request);

        Task<ApiResponse<LeaveRequestResponseDto>> UpdateLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId,
            UpdateLeaveRequestDto request);

        Task<ApiResponse<bool>> CancelLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId);

        Task<ApiResponse<PagedResult<LeaveRequestResponseDto>>> GetMyLeaveRequestsAsync(
            CurrentUser currentUser,
            LeaveRequestFilterDto filter);

        Task<ApiResponse<PagedResult<LeaveRequestResponseDto>>> GetLeaveRequestsAsync(
            CurrentUser currentUser,
            LeaveRequestFilterDto filter);

        Task<ApiResponse<List<LeaveRequestResponseDto>>> GetPendingLeaveRequestsAsync(
            CurrentUser currentUser);

        Task<ApiResponse<LeaveRequestResponseDto>> ApproveOrRejectLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId,
            ApproveLeaveRequestDto request);

        Task<ApiResponse<List<LeaveTypeResponseDto>>> GetLeaveTypesAsync(
            bool? isActive = null);

        Task<ApiResponse<LeaveTypeResponseDto>> CreateLeaveTypeAsync(
            CurrentUser currentUser,
            CreateLeaveTypeRequestDto request);

        Task<ApiResponse<LeaveTypeResponseDto>> UpdateLeaveTypeAsync(
            CurrentUser currentUser,
            int leaveTypeId,
            UpdateLeaveTypeRequestDto request);

        Task<ApiResponse<bool>> DeactivateLeaveTypeAsync(
            CurrentUser currentUser,
            int leaveTypeId);

        Task<ApiResponse<bool>> ActivateLeaveTypeAsync(
            CurrentUser currentUser,
            int leaveTypeId);

        Task<ApiResponse<LeaveBalanceResponseDto>> SetLeaveBalanceAsync(
            CurrentUser currentUser,
            SetLeaveBalanceRequestDto request);

        Task<ApiResponse<List<LeaveBalanceResponseDto>>> GetLeaveBalancesByEmployeeAsync(
            CurrentUser currentUser,
            int employeeId,
            int? year);
    }
}