using HRM.Business.Common;
using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.Interfaces
{
    public interface ILeaveApiClient
    {
        Task<ApiResponse<PagedResult<LeaveRequestResponseDto>>> GetMyLeaveRequestsAsync(
            int? month = null, int? year = null, string? status = null,
            int pageNumber = 1, int pageSize = 50);

        Task<ApiResponse<List<LeaveRequestResponseDto>>> GetPendingLeaveRequestsAsync();

        Task<ApiResponse<List<LeaveTypeResponseDto>>> GetLeaveTypesAsync(bool? isActive = null);

        Task<ApiResponse<LeaveRequestResponseDto>> CreateLeaveRequestAsync(CreateLeaveRequestDto model);

        Task<ApiResponse<LeaveRequestResponseDto>> UpdateLeaveRequestAsync(int leaveRequestId, UpdateLeaveRequestDto model);

        Task<ApiResponse<bool>> CancelLeaveRequestAsync(int leaveRequestId);

        Task<ApiResponse<LeaveRequestResponseDto>> ApproveLeaveRequestAsync(
            int leaveRequestId, ApproveLeaveRequestDto model);

        Task<ApiResponse<LeaveTypeResponseDto>> CreateLeaveTypeAsync(CreateLeaveTypeRequestDto model);

        Task<ApiResponse<LeaveTypeResponseDto>> UpdateLeaveTypeAsync(int leaveTypeId, UpdateLeaveTypeRequestDto model);

        Task<ApiResponse<bool>> DeactivateLeaveTypeAsync(int leaveTypeId);

        Task<ApiResponse<bool>> ActivateLeaveTypeAsync(int leaveTypeId);

        Task<ApiResponse<List<LeaveBalanceResponseDto>>> GetLeaveBalancesAsync(int employeeId, int? year = null);

        Task<ApiResponse<LeaveBalanceResponseDto>> SetLeaveBalanceAsync(SetLeaveBalanceRequestDto model);
    }
}
