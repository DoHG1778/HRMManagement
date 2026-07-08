using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IPayrollRepository : IGenericRepository<Payroll>
    {
        Task<Payroll?> GetPayrollByEmployeeAndPeriodAsync(
            int employeeId,
            int payrollMonth,
            int payrollYear);

        Task<Payroll?> GetPayrollDetailAsync(int payrollId);

        Task<List<Payroll>> GetPayrollsByPeriodAsync(int payrollMonth, int payrollYear);

        Task<List<Payroll>> GetPayrollsByFilterAsync(
            int payrollMonth,
            int payrollYear,
            int? departmentId,
            int? employeeId,
            string? status);

        Task<bool> IsPayrollExistsAsync(
            int employeeId,
            int payrollMonth,
            int payrollYear);
    }
}