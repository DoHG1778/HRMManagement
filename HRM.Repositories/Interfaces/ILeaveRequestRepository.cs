using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface ILeaveRequestRepository : IGenericRepository<LeaveRequest>
    {
        Task<List<LeaveRequest>> GetPersonalLeaveRequestsAsync(
            int employeeId,
            string? status,
            int? month,
            int? year);

        Task<List<LeaveRequest>> GetPendingLeaveRequestsByManagerAsync(int managerEmployeeId);

        Task<bool> HasOverlappingLeaveRequestAsync(
            int employeeId,
            DateTime startDate,
            DateTime endDate,
            int? excludeLeaveRequestId = null);

        Task<bool> IsLeaveRequestOwnedByEmployeeAsync(int leaveRequestId, int employeeId);

        Task<bool> IsLeaveRequestUnderManagerAsync(int leaveRequestId, int managerEmployeeId);
    }
}