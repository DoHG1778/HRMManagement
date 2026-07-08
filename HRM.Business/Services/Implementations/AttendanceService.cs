using HRM.Business.Common;
using HRM.Business.DTOs.Attendances;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<CheckInResponseDto>> CheckInAsync(
            CurrentUser currentUser)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<CheckOutResponseDto>> CheckOutAsync(
            CurrentUser currentUser)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<AttendanceResponseDto>>> GetMyAttendanceHistoryAsync(
            CurrentUser currentUser,
            AttendanceFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<AttendanceResponseDto>>> GetAttendanceSheetAsync(
            CurrentUser currentUser,
            AttendanceFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> CreateAdjustmentRequestAsync(
            CurrentUser currentUser,
            CreateAttendanceAdjustmentRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> UpdateAdjustmentRequestAsync(
            CurrentUser currentUser,
            int adjustmentId,
            UpdateAttendanceAdjustmentRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> CancelAdjustmentRequestAsync(
            CurrentUser currentUser,
            int adjustmentId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> ApproveOrRejectAdjustmentAsync(
            CurrentUser currentUser,
            int adjustmentId,
            ApproveAttendanceAdjustmentRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<AttendanceAdjustmentResponseDto>>> GetPendingAdjustmentRequestsAsync(
            CurrentUser currentUser)
        {
            throw new NotImplementedException();
        }
    }
}