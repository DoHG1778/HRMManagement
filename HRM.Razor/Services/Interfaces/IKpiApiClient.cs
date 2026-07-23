using HRM.Business.Common;
using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels.Kpis;

namespace HRM.Razor.Services.Interfaces
{
    public interface IKpiApiClient
    {
        Task<ApiResponse<List<KpiViewModel>>>
            GetKpisAsync(bool? isActive = null);

        Task<ApiResponse<KpiViewModel>>
            GetKpiByIdAsync(int kpiId);

        Task<ApiResponse<KpiViewModel>>
            CreateKpiAsync(CreateKpiViewModel model);

        Task<ApiResponse<KpiViewModel>>
            UpdateKpiAsync(
                int kpiId,
                UpdateKpiViewModel model);

        Task<ApiResponse<bool>>
            DeactivateKpiAsync(int kpiId);

        Task<ApiResponse<KpiAssignmentViewModel>>
            AssignKpiAsync(AssignKpiViewModel model);

        Task<ApiResponse<KpiAssignmentViewModel>>
            UpdateKpiProgressAsync(
                int assignmentId,
                UpdateKpiProgressViewModel model);

        Task<ApiResponse<KpiAssignmentViewModel>>
            EvaluateKpiAsync(
                int assignmentId,
                EvaluateKpiViewModel model);

        Task<ApiResponse<
            HRM.Razor.Models.PagedResultModel<KpiAssignmentViewModel>>>
            GetMyKpiAssignmentsAsync(
                int? kpiId = null,
                string? status = null,
                DateOnly? fromDate = null,
                DateOnly? toDate = null,
                int pageNumber = 1,
                int pageSize = 10);

        Task<ApiResponse<
            HRM.Razor.Models.PagedResultModel<KpiAssignmentViewModel>>>
            GetKpiAssignmentsAsync(
                int? employeeId = null,
                int? kpiId = null,
                string? status = null,
                DateOnly? fromDate = null,
                DateOnly? toDate = null,
                int pageNumber = 1,
                int pageSize = 10);
    }
}