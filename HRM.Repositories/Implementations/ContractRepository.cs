using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class ContractRepository : GenericRepository<Contract>, IContractRepository
    {
        public ContractRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Contract?> GetActiveContractByEmployeeIdAsync(int employeeId)
        {
            return await _context.Contracts
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c =>
                    c.EmployeeId == employeeId &&
                    c.Status == "ACTIVE");
        }

        public async Task<List<Contract>> GetContractsByEmployeeIdAsync(int employeeId)
        {
            return await _context.Contracts
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();
        }

        public async Task<List<Contract>> GetContractsExpiringSoonAsync(DateTime fromDate, DateTime toDate)
        {
            var from = DateOnly.FromDateTime(fromDate);
            var to = DateOnly.FromDateTime(toDate);

            return await _context.Contracts
                .Include(c => c.Employee)
                .Where(c =>
                    c.EndDate != null &&
                    c.EndDate >= from &&
                    c.EndDate <= to &&
                    c.Status == "ACTIVE")
                .OrderBy(c => c.EndDate)
                .ToListAsync();
        }

        public async Task<bool> HasActiveContractAsync(int employeeId)
        {
            return await _context.Contracts
                .AnyAsync(c =>
                    c.EmployeeId == employeeId &&
                    c.Status == "ACTIVE");
        }
    }
}