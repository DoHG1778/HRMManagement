using HRM.Business.Common;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Models.ViewModels.Payroll;

namespace HRM.Razor.Services.Interfaces
{
    public interface IPayrollApiClient
    {
        Task<ApiResponse<PagedResultModel<PayrollItemModel>>> GetPayrollsAsync(
            int? month = null,
            int? year = null,
            int? departmentId = null,
            int? employeeId = null,
            string? status = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<ApiResponse<PayrollItemModel>> GetPayrollByIdAsync(int payrollId);

        Task<ApiResponse<List<PayrollItemModel>>> GenerateMonthlyPayrollAsync(GeneratePayrollViewModel model);

        Task<ApiResponse<List<PayrollItemModel>>> CalculatePayrollAsync(CalculatePayrollViewModel model);

        Task<ApiResponse<PayrollItemModel>> UpdatePayrollDetailAsync(int payrollId, List<PayrollDetailItemModel> items);

        Task<ApiResponse<bool>> ConfirmPayrollAsync(int month, int year, int? departmentId = null, int? employeeId = null);

        Task<ApiResponse<PayrollItemModel>> GetMyPayslipAsync(int month, int year);

        Task<ApiResponse<PayrollReportViewModel>> GetPayrollReportAsync(int month, int year, int? departmentId = null, int? employeeId = null);
    }
}
