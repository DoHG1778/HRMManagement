using HRM.Models.Entities;
using HRM.Repositories.Interfaces;

namespace HRM.Repositories.UnitOfWork
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }

        IEmployeeRepository Employees { get; }

        IDepartmentRepository Departments { get; }

        IPositionRepository Positions { get; }

        IContractRepository Contracts { get; }

        IAttendanceRepository Attendances { get; }

        IGenericRepository<AttendanceAdjustment> AttendanceAdjustments { get; }

        ILeaveRequestRepository LeaveRequests { get; }

        IOvertimeRequestRepository OvertimeRequests { get; }

        IPayrollRepository Payrolls { get; }

        IGenericRepository<PayrollDetail> PayrollDetails { get; }

        IKpiRepository Kpis { get; }

        INotificationRepository Notifications { get; }

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}