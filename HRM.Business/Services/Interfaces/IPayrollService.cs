using HRM.Business.Common;
using HRM.Business.DTOs.Payrolls;

namespace HRM.Business.Services.Interfaces
{
    public interface IPayrollService
    {
        Task<ApiResponse<List<PayrollResponseDto>>> GenerateMonthlyPayrollAsync(
            CurrentUser currentUser,
            GeneratePayrollRequestDto request);

        Task<ApiResponse<List<PayrollResponseDto>>> CalculatePayrollAsync(
            CurrentUser currentUser,
            CalculatePayrollDto request);

        Task<ApiResponse<PayrollResponseDto>> GetPayrollDetailAsync(
            CurrentUser currentUser,
            int payrollId);

        Task<ApiResponse<PagedResult<PayrollResponseDto>>> GetPayrollsAsync(
            CurrentUser currentUser,
            PayrollFilterDto filter);

        Task<ApiResponse<PayrollResponseDto>> UpdatePayrollDetailAsync(
            CurrentUser currentUser,
            int payrollId,
            UpdatePayrollDetailRequestDto request);

        Task<ApiResponse<bool>> ConfirmPayrollAsync(
            CurrentUser currentUser,
            ConfirmPayrollRequestDto request);

        Task<ApiResponse<bool>> PayPayrollAsync(
            CurrentUser currentUser,
            ConfirmPayrollRequestDto request);

        Task<ApiResponse<PayrollResponseDto>> GetMyPayslipAsync(
            CurrentUser currentUser,
            int payrollMonth,
            int payrollYear);

        Task<ApiResponse<PayrollReportDto>> ExportPayrollReportAsync(
            CurrentUser currentUser,
            PayrollFilterDto filter);
    }
}