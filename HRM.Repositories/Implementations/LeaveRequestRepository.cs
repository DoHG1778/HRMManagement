using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class LeaveRequestRepository : GenericRepository<LeaveRequest>, ILeaveRequestRepository
    {
        public LeaveRequestRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<LeaveRequest>> GetPersonalLeaveRequestsAsync(
            int employeeId,
            string? status,
            int? month,
            int? year)
        {
            var query = _context.LeaveRequests
                .Include(l => l.LeaveType)
                .Where(l => l.EmployeeId == employeeId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            if (month.HasValue)
            {
                query = query.Where(l => l.StartDate.Month == month.Value);
            }

            if (year.HasValue)
            {
                query = query.Where(l => l.StartDate.Year == year.Value);
            }

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<LeaveRequest>> GetPendingLeaveRequestsByManagerAsync(int managerEmployeeId)
        {
            return await _context.LeaveRequests
                .Include(l => l.Employee)
                .Include(l => l.LeaveType)
                .Where(l =>
                    l.Status == "PENDING" &&
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == l.EmployeeId &&
                        ea.EndDate == null &&
                        ea.Department.ManagerEmployeeId == managerEmployeeId))
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingLeaveRequestAsync(
            int employeeId,
            DateTime startDate,
            DateTime endDate,
            int? excludeLeaveRequestId = null)
        {
            var start = DateOnly.FromDateTime(startDate);
            var end = DateOnly.FromDateTime(endDate);

            var query = _context.LeaveRequests
                .Where(l =>
                    l.EmployeeId == employeeId &&
                    (l.Status == "PENDING" || l.Status == "APPROVED") &&
                    l.StartDate <= end &&
                    l.EndDate >= start);

            if (excludeLeaveRequestId.HasValue)
            {
                query = query.Where(l => l.LeaveRequestId != excludeLeaveRequestId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> IsLeaveRequestOwnedByEmployeeAsync(int leaveRequestId, int employeeId)
        {
            return await _context.LeaveRequests
                .AnyAsync(l =>
                    l.LeaveRequestId == leaveRequestId &&
                    l.EmployeeId == employeeId);
        }

        public async Task<bool> IsLeaveRequestUnderManagerAsync(int leaveRequestId, int managerEmployeeId)
        {
            return await _context.LeaveRequests
                .AnyAsync(l =>
                    l.LeaveRequestId == leaveRequestId &&
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == l.EmployeeId &&
                        ea.EndDate == null &&
                        ea.Department.ManagerEmployeeId == managerEmployeeId));
        }
    }
}