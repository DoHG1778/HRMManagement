using HRM.Business.Common;
using HRM.Business.DTOs.Payrolls;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class PayrollService : IPayrollService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PayrollService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<PayrollResponseDto>>> GenerateMonthlyPayrollAsync(
            CurrentUser currentUser,
            GeneratePayrollRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<PayrollResponseDto>>> CalculatePayrollAsync(
            CurrentUser currentUser,
            int payrollMonth,
            int payrollYear)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PayrollResponseDto>> GetPayrollDetailAsync(
            CurrentUser currentUser,
            int payrollId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PagedResult<PayrollResponseDto>>> GetPayrollsAsync(
            CurrentUser currentUser,
            PayrollFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PayrollResponseDto>> UpdatePayrollDetailAsync(
            CurrentUser currentUser,
            int payrollId,
            UpdatePayrollDetailRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> ConfirmPayrollAsync(
            CurrentUser currentUser,
            ConfirmPayrollRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PayrollResponseDto>> GetMyPayslipAsync(
            CurrentUser currentUser,
            int payrollMonth,
            int payrollYear)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PayrollReportDto>> ExportPayrollReportAsync(
            CurrentUser currentUser,
            PayrollFilterDto filter)
        {
            throw new NotImplementedException();
        }
    }
}