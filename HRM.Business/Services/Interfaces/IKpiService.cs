using HRM.Business.Common;
using HRM.Business.DTOs.Kpis;

namespace HRM.Business.Services.Interfaces
{
    public interface IKpiService
    {
        Task<ApiResponse<List<KpiResponseDto>>> GetKpisAsync(
            bool? isActive = null);

        Task<ApiResponse<KpiResponseDto>> GetKpiByIdAsync(
            int kpiId);

        Task<ApiResponse<KpiResponseDto>> CreateKpiAsync(
            CurrentUser currentUser,
            CreateKpiRequestDto request);

        Task<ApiResponse<KpiResponseDto>> UpdateKpiAsync(
            CurrentUser currentUser,
            int kpiId,
            UpdateKpiRequestDto request);

        Task<ApiResponse<bool>> DeactivateKpiAsync(
            CurrentUser currentUser,
            int kpiId);

        Task<ApiResponse<KpiAssignmentResponseDto>> AssignKpiAsync(
            CurrentUser currentUser,
            AssignKpiRequestDto request);

        Task<ApiResponse<KpiAssignmentResponseDto>> UpdateKpiProgressAsync(
            CurrentUser currentUser,
            int assignmentId,
            UpdateKpiProgressRequestDto request);

        Task<ApiResponse<KpiAssignmentResponseDto>> EvaluateKpiAsync(
            CurrentUser currentUser,
            int assignmentId,
            EvaluateKpiRequestDto request);

        Task<ApiResponse<PagedResult<KpiAssignmentResponseDto>>> GetMyKpiAssignmentsAsync(
            CurrentUser currentUser,
            KpiAssignmentFilterDto filter);

        Task<ApiResponse<PagedResult<KpiAssignmentResponseDto>>> GetKpiAssignmentsAsync(
            CurrentUser currentUser,
            KpiAssignmentFilterDto filter);
    }
}