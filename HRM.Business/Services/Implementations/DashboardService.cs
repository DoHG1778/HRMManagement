using HRM.Business.Common;
using HRM.Business.DTOs.Dashboards;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync(
            CurrentUser currentUser,
            int? month,
            int? year)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<EmployeeDashboardDto>> GetEmployeeDashboardAsync(
            CurrentUser currentUser,
            int? month,
            int? year)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<ManagerDashboardDto>> GetManagerDashboardAsync(
            CurrentUser currentUser,
            int? month,
            int? year)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PayrollDashboardDto>> GetPayrollDashboardAsync(
            CurrentUser currentUser,
            int payrollMonth,
            int payrollYear)
        {
            throw new NotImplementedException();
        }
    }
}