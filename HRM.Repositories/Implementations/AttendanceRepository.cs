using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
    {
        public AttendanceRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime attendanceDate)
        {
            var date = DateOnly.FromDateTime(attendanceDate);

            return await _context.Attendances
                .FirstOrDefaultAsync(a =>
                    a.EmployeeId == employeeId &&
                    a.AttendanceDate == date);
        }

        public async Task<List<Attendance>> GetPersonalAttendanceHistoryAsync(
            int employeeId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var query = _context.Attendances
                .Where(a => a.EmployeeId == employeeId);

            if (fromDate.HasValue)
            {
                var from = DateOnly.FromDateTime(fromDate.Value);
                query = query.Where(a => a.AttendanceDate >= from);
            }

            if (toDate.HasValue)
            {
                var to = DateOnly.FromDateTime(toDate.Value);
                query = query.Where(a => a.AttendanceDate <= to);
            }

            return await query
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetAttendanceSheetAsync(
    int? departmentId,
    int? employeeId,
    DateTime? fromDate,
    DateTime? toDate,
    string? status)
        {
            var query = _context.Attendances
                .Include(a => a.Employee)
                .AsQueryable();

            if (departmentId.HasValue)
            {
                query = query.Where(a =>
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == a.EmployeeId &&
                        ea.DepartmentId == departmentId.Value &&
                        ea.EndDate == null));
            }

            if (employeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            if (fromDate.HasValue)
            {
                var from = DateOnly.FromDateTime(fromDate.Value);
                query = query.Where(a => a.AttendanceDate >= from);
            }

            if (toDate.HasValue)
            {
                var to = DateOnly.FromDateTime(toDate.Value);
                query = query.Where(a => a.AttendanceDate <= to);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            return await query
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<List<Attendance>> GetAttendanceByManagerAsync(
    int managerEmployeeId,
    DateTime? fromDate,
    DateTime? toDate)
        {
            var query = _context.Attendances
                .Include(a => a.Employee)
                .Where(a =>
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == a.EmployeeId &&
                        ea.EndDate == null &&
                        ea.Department.ManagerEmployeeId == managerEmployeeId));

            if (fromDate.HasValue)
            {
                var from = DateOnly.FromDateTime(fromDate.Value);
                query = query.Where(a => a.AttendanceDate >= from);
            }

            if (toDate.HasValue)
            {
                var to = DateOnly.FromDateTime(toDate.Value);
                query = query.Where(a => a.AttendanceDate <= to);
            }

            return await query
                .OrderByDescending(a => a.AttendanceDate)
                .ToListAsync();
        }

        public async Task<bool> HasAttendanceTodayAsync(int employeeId, DateTime today)
        {
            var date = DateOnly.FromDateTime(today);

            return await _context.Attendances
                .AnyAsync(a =>
                    a.EmployeeId == employeeId &&
                    a.AttendanceDate == date);
        }
    }
}