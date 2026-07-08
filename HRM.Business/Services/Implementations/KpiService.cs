using HRM.Business.Common;
using HRM.Business.DTOs.Kpis;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class KpiService : IKpiService
    {
        private readonly IUnitOfWork _unitOfWork;

        public KpiService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<KpiResponseDto>>> GetKpisAsync(
            bool? isActive = null)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<KpiResponseDto>> GetKpiByIdAsync(
            int kpiId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<KpiResponseDto>> CreateKpiAsync(
            CurrentUser currentUser,
            CreateKpiRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<KpiResponseDto>> UpdateKpiAsync(
            CurrentUser currentUser,
            int kpiId,
            UpdateKpiRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> DeactivateKpiAsync(
            CurrentUser currentUser,
            int kpiId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<KpiAssignmentResponseDto>> AssignKpiAsync(
            CurrentUser currentUser,
            AssignKpiRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<KpiAssignmentResponseDto>> UpdateKpiProgressAsync(
            CurrentUser currentUser,
            int assignmentId,
            UpdateKpiProgressRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<KpiAssignmentResponseDto>> EvaluateKpiAsync(
            CurrentUser currentUser,
            int assignmentId,
            EvaluateKpiRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<KpiAssignmentResponseDto>>> GetMyKpiAssignmentsAsync(
            CurrentUser currentUser,
            KpiAssignmentFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<KpiAssignmentResponseDto>>> GetKpiAssignmentsAsync(
            CurrentUser currentUser,
            KpiAssignmentFilterDto filter)
        {
            throw new NotImplementedException();
        }
    }
}