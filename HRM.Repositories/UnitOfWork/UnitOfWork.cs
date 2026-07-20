using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Implementations;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace HRM.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public IUserRepository Users { get; }
        public IGenericRepository<Role> Roles { get; }
        public IEmployeeRepository Employees { get; }
        public IDepartmentRepository Departments { get; }
        public IPositionRepository Positions { get; }
        public IContractRepository Contracts { get; }
        public IAttendanceRepository Attendances { get; }
        public IGenericRepository<AttendanceAdjustment> AttendanceAdjustments { get; }
        public ILeaveRequestRepository LeaveRequests { get; }
        public IOvertimeRequestRepository OvertimeRequests { get; }
        public IPayrollRepository Payrolls { get; }
        public IGenericRepository<PayrollDetail> PayrollDetails { get; }
        public IKpiRepository Kpis { get; }
        public INotificationRepository Notifications { get; }

        public UnitOfWork(
            AppDbContext context,
            IUserRepository users,
            IEmployeeRepository employees,
            IDepartmentRepository departments,
            IPositionRepository positions,
            IContractRepository contracts,
            IAttendanceRepository attendances,
            ILeaveRequestRepository leaveRequests,
            IOvertimeRequestRepository overtimeRequests,
            IPayrollRepository payrolls,
            IKpiRepository kpis,
            INotificationRepository notifications)
        {
            _context = context;
            Users = users;
            Roles = new GenericRepository<Role>(context);
            Employees = employees;
            Departments = departments;
            Positions = positions;
            Contracts = contracts;
            Attendances = attendances;
            AttendanceAdjustments = new GenericRepository<AttendanceAdjustment>(context);
            LeaveRequests = leaveRequests;
            OvertimeRequests = overtimeRequests;
            Payrolls = payrolls;
            PayrollDetails = new GenericRepository<PayrollDetail>(context);
            Kpis = kpis;
            Notifications = notifications;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                return;
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                return;
            }

            try
            {
                await _transaction.CommitAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                return;
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}