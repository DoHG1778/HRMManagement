using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class PayrollRepository : GenericRepository<Payroll>, IPayrollRepository
    {
        public PayrollRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Payroll?> GetPayrollByEmployeeAndPeriodAsync(
            int employeeId,
            int payrollMonth,
            int payrollYear)
        {
            return await _context.Payrolls
                .Include(p => p.PayrollDetails)
                .FirstOrDefaultAsync(p =>
                    p.EmployeeId == employeeId &&
                    p.PayrollMonth == payrollMonth &&
                    p.PayrollYear == payrollYear);
        }

        public async Task<Payroll?> GetPayrollDetailAsync(int payrollId)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.PayrollDetails)
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId);
        }

        public async Task<List<Payroll>> GetPayrollsByPeriodAsync(int payrollMonth, int payrollYear)
        {
            return await _context.Payrolls
                .Include(p => p.Employee)
                .Where(p =>
                    p.PayrollMonth == payrollMonth &&
                    p.PayrollYear == payrollYear)
                .OrderBy(p => p.Employee.FullName)
                .ToListAsync();
        }

        public async Task<List<Payroll>> GetPayrollsByFilterAsync(
    int payrollMonth,
    int payrollYear,
    int? departmentId,
    int? employeeId,
    string? status)
        {
            var query = _context.Payrolls
                .Include(p => p.Employee)
                .Include(p => p.PayrollDetails)
                .Where(p =>
                    p.PayrollMonth == payrollMonth &&
                    p.PayrollYear == payrollYear);

            if (departmentId.HasValue)
            {
                query = query.Where(p =>
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == p.EmployeeId &&
                        ea.DepartmentId == departmentId.Value &&
                        ea.EndDate == null));
            }

            if (employeeId.HasValue)
            {
                query = query.Where(p => p.EmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(p => p.Status == status);
            }

            return await query
                .OrderBy(p => p.Employee.FullName)
                .ToListAsync();
        }

        public async Task<bool> IsPayrollExistsAsync(
            int employeeId,
            int payrollMonth,
            int payrollYear)
        {
            return await _context.Payrolls
                .AnyAsync(p =>
                    p.EmployeeId == employeeId &&
                    p.PayrollMonth == payrollMonth &&
                    p.PayrollYear == payrollYear);
        }
    }
}