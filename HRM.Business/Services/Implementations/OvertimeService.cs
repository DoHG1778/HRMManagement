using HRM.Business.Common;
using HRM.Business.DTOs.Overtimes;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class OvertimeService : IOvertimeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OvertimeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> CreateOvertimeRequestAsync(
            CurrentUser currentUser,
            CreateOvertimeRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> UpdateOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId,
            UpdateOvertimeRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> CancelOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<OvertimeRequestResponseDto>>> GetMyOvertimeRequestsAsync(
            CurrentUser currentUser,
            OvertimeRequestFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<OvertimeRequestResponseDto>>> GetOvertimeRequestsAsync(
            CurrentUser currentUser,
            OvertimeRequestFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<OvertimeRequestResponseDto>>> GetPendingOvertimeRequestsAsync(
            CurrentUser currentUser)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> ApproveOrRejectOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId,
            ApproveOvertimeRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<OvertimeRequestResponseDto>>> GetApprovedOvertimeForPayrollAsync(
            CurrentUser currentUser,
            int employeeId,
            int month,
            int year)
        {
            throw new NotImplementedException();
        }
    }
}