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

        ILeaveRequestRepository LeaveRequests { get; }

        IOvertimeRequestRepository OvertimeRequests { get; }

        IPayrollRepository Payrolls { get; }

        IKpiRepository Kpis { get; }

        INotificationRepository Notifications { get; }

        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();

        Task CommitTransactionAsync();

        Task RollbackTransactionAsync();
    }
}