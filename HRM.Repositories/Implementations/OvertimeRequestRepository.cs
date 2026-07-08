using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class OvertimeRequestRepository : GenericRepository<OvertimeRequest>, IOvertimeRequestRepository
    {
        public OvertimeRequestRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<OvertimeRequest>> GetPersonalOvertimeRequestsAsync(
            int employeeId,
            string? status,
            int? month,
            int? year)
        {
            var query = _context.OvertimeRequests
                .Where(o => o.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            if (month.HasValue)
            {
                query = query.Where(o => o.Otdate.Month == month.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(o => o.Otdate.Year == year.Value);
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<OvertimeRequest>> GetPendingOvertimeRequestsByManagerAsync(int managerEmployeeId)
        {
            return await _context.OvertimeRequests
                .Include(o => o.Employee)
                .Where(o =>
                    o.Status == "PENDING" &&
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == o.EmployeeId &&
                        ea.EndDate == null &&
                        ea.Department.ManagerEmployeeId == managerEmployeeId))
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<OvertimeRequest>> GetApprovedOvertimeForPayrollAsync(
            int employeeId,
            int month,
            int year)
        {
            return await _context.OvertimeRequests
                .Where(o =>
                    o.EmployeeId == employeeId &&
                    o.Status == "APPROVED" &&
                    !o.IsTransferredToPayroll &&
                    o.Otdate.Month == month &&
                    o.Otdate.Year == year)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingOvertimeRequestAsync(
            int employeeId,
            DateTime startTime,
            DateTime endTime,
            int? excludeOvertimeRequestId = null)
        {
            var query = _context.OvertimeRequests
                .Where(o =>
                    o.EmployeeId == employeeId &&
                    (o.Status == "PENDING" || o.Status == "APPROVED") &&
                    o.StartTime < endTime &&
                    o.EndTime > startTime);

            if (excludeOvertimeRequestId.HasValue)
            {
                query = query.Where(o =>
                    EF.Property<int>(o, "Otid") != excludeOvertimeRequestId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsOvertimeRequestOwnedByEmployeeAsync(int overtimeRequestId, int employeeId)
        {
            return await _context.OvertimeRequests
                .AnyAsync(o =>
                    EF.Property<int>(o, "Otid") == overtimeRequestId &&
                    o.EmployeeId == employeeId);
        }

        public async Task<bool> IsOvertimeRequestUnderManagerAsync(int overtimeRequestId, int managerEmployeeId)
        {
            return await _context.OvertimeRequests
                .AnyAsync(o =>
                    o.Otid == overtimeRequestId &&
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == o.EmployeeId &&
                        ea.EndDate == null &&
                        ea.Department.ManagerEmployeeId == managerEmployeeId));
        }
    }
}