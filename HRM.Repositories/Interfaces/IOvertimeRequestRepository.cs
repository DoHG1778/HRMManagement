using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IOvertimeRequestRepository : IGenericRepository<OvertimeRequest>
    {
        Task<List<OvertimeRequest>> GetPersonalOvertimeRequestsAsync(
            int employeeId,
            string? status,
            int? month,
            int? year);

        Task<List<OvertimeRequest>> GetPendingOvertimeRequestsByManagerAsync(int managerEmployeeId);

        Task<List<OvertimeRequest>> GetApprovedOvertimeForPayrollAsync(
            int employeeId,
            int month,
            int year);

        Task<bool> HasOverlappingOvertimeRequestAsync(
            int employeeId,
            DateTime startTime,
            DateTime endTime,
            int? excludeOvertimeRequestId = null);

        Task<bool> IsOvertimeRequestOwnedByEmployeeAsync(int overtimeRequestId, int employeeId);

        Task<bool> IsOvertimeRequestUnderManagerAsync(int overtimeRequestId, int managerEmployeeId);
    }
}