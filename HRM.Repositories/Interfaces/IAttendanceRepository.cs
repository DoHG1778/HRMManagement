using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IAttendanceRepository : IGenericRepository<Attendance>
    {
        Task<Attendance?> GetByEmployeeAndDateAsync(int employeeId, DateTime attendanceDate);

        Task<List<Attendance>> GetPersonalAttendanceHistoryAsync(
            int employeeId,
            DateTime? fromDate,
            DateTime? toDate);

        Task<List<Attendance>> GetAttendanceSheetAsync(
            int? departmentId,
            int? employeeId,
            DateTime? fromDate,
            DateTime? toDate,
            string? status);

        Task<List<Attendance>> GetAttendanceByManagerAsync(
            int managerEmployeeId,
            DateTime? fromDate,
            DateTime? toDate);

        Task<bool> HasAttendanceTodayAsync(int employeeId, DateTime today);
    }
}