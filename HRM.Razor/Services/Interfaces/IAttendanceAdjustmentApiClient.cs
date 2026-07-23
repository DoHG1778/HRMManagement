using HRM.Business.Common;
using HRM.Razor.Models.ViewModels.AttendanceAdjustments;

namespace HRM.Razor.Services.Interfaces
{
    public interface IAttendanceAdjustmentApiClient
    {
        Task<ApiResponse<List<AttendanceAdjustmentItemModel>>> GetMyAdjustmentsAsync(
            string? status = null,
            int? month = null,
            int? year = null);

        Task<ApiResponse<List<AdjustableAttendanceModel>>> GetAdjustableAttendancesAsync(
            int? month = null,
            int? year = null);

        Task<ApiResponse<AttendanceAdjustmentItemModel>> GetAdjustmentByIdAsync(int id);

        Task<ApiResponse<AttendanceAdjustmentItemModel>> CreateAdjustmentAsync(CreateAttendanceAdjustmentViewModel model);

        Task<ApiResponse<AttendanceAdjustmentItemModel>> UpdateAdjustmentAsync(int id, UpdateAttendanceAdjustmentViewModel model);

        Task<ApiResponse<bool>> CancelAdjustmentAsync(int id);

        Task<ApiResponse<List<AttendanceAdjustmentItemModel>>> GetPendingAdjustmentsAsync(
            int? employeeId = null,
            int? month = null,
            int? year = null);

        Task<ApiResponse<AttendanceAdjustmentItemModel>> ApproveAdjustmentAsync(int id);

        Task<ApiResponse<AttendanceAdjustmentItemModel>> RejectAdjustmentAsync(int id, string rejectionReason);
    }
}
