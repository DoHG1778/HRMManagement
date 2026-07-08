using HRM.Business.Common;
using HRM.Business.DTOs.Dashboards;

namespace HRM.Business.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync(
            CurrentUser currentUser,
            int? month,
            int? year);

        Task<ApiResponse<EmployeeDashboardDto>> GetEmployeeDashboardAsync(
            CurrentUser currentUser,
            int? month,
            int? year);

        Task<ApiResponse<ManagerDashboardDto>> GetManagerDashboardAsync(
            CurrentUser currentUser,
            int? month,
            int? year);

        Task<ApiResponse<PayrollDashboardDto>> GetPayrollDashboardAsync(
            CurrentUser currentUser,
            int payrollMonth,
            int payrollYear);
    }
}