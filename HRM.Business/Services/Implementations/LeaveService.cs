using HRM.Business.Common;
using HRM.Business.DTOs.Leaves;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class LeaveService : ILeaveService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeaveService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> CreateLeaveRequestAsync(
            CurrentUser currentUser,
            CreateLeaveRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> UpdateLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId,
            UpdateLeaveRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> CancelLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<LeaveRequestResponseDto>>> GetMyLeaveRequestsAsync(
            CurrentUser currentUser,
            LeaveRequestFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<LeaveRequestResponseDto>>> GetLeaveRequestsAsync(
            CurrentUser currentUser,
            LeaveRequestFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<LeaveRequestResponseDto>>> GetPendingLeaveRequestsAsync(
            CurrentUser currentUser)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> ApproveOrRejectLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId,
            ApproveLeaveRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<LeaveTypeResponseDto>>> GetLeaveTypesAsync(
            bool? isActive = null)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<LeaveTypeResponseDto>> CreateLeaveTypeAsync(
            CurrentUser currentUser,
            CreateLeaveTypeRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<LeaveTypeResponseDto>> UpdateLeaveTypeAsync(
            CurrentUser currentUser,
            int leaveTypeId,
            UpdateLeaveTypeRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> DeactivateLeaveTypeAsync(
            CurrentUser currentUser,
            int leaveTypeId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<LeaveBalanceResponseDto>> SetLeaveBalanceAsync(
            CurrentUser currentUser,
            SetLeaveBalanceRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<LeaveBalanceResponseDto>>> GetLeaveBalancesByEmployeeAsync(
            CurrentUser currentUser,
            int employeeId,
            int? year)
        {
            throw new NotImplementedException();
        }
    }
}