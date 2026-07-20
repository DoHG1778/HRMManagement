using HRM.Business.Common;
using HRM.Business.DTOs.Attendances;

namespace HRM.Business.Services.Interfaces
{
    public interface IAttendanceService
    {
        Task<ApiResponse<CheckInResponseDto>> CheckInAsync(
            CurrentUser currentUser);

        Task<ApiResponse<CheckOutResponseDto>> CheckOutAsync(
            CurrentUser currentUser);

        Task<ApiResponse<PagedResult<AttendanceResponseDto>>> GetMyAttendanceHistoryAsync(
            CurrentUser currentUser,
            AttendanceFilterDto filter);

        Task<ApiResponse<PagedResult<AttendanceResponseDto>>> GetAttendanceSheetAsync(
            CurrentUser currentUser,
            AttendanceFilterDto filter);

        Task<ApiResponse<AttendanceAdjustmentResponseDto>> CreateAdjustmentRequestAsync(
            CurrentUser currentUser,
            CreateAttendanceAdjustmentRequestDto request);

        Task<ApiResponse<AttendanceAdjustmentResponseDto>> UpdateAdjustmentRequestAsync(
            CurrentUser currentUser,
            int adjustmentId,
            UpdateAttendanceAdjustmentRequestDto request);

        Task<ApiResponse<bool>> CancelAdjustmentRequestAsync(
            CurrentUser currentUser,
            int adjustmentId);

        Task<ApiResponse<AttendanceAdjustmentResponseDto>> ApproveOrRejectAdjustmentAsync(
            CurrentUser currentUser,
            int adjustmentId,
            ApproveAttendanceAdjustmentRequestDto request);

        Task<ApiResponse<List<AttendanceAdjustmentResponseDto>>> GetPendingAdjustmentRequestsAsync(
            CurrentUser currentUser,
            int? employeeId = null,
            int? month = null,
            int? year = null);

        Task<ApiResponse<List<AttendanceAdjustmentResponseDto>>> GetMyAdjustmentRequestsAsync(
            CurrentUser currentUser,
            string? status = null,
            int? month = null,
            int? year = null);

        Task<ApiResponse<List<AdjustableAttendanceDto>>> GetAdjustableAttendancesAsync(
            CurrentUser currentUser,
            int? month = null,
            int? year = null);

        Task<ApiResponse<AttendanceAdjustmentResponseDto>> GetAdjustmentDetailAsync(
            CurrentUser currentUser,
            int adjustmentId);
    }
}