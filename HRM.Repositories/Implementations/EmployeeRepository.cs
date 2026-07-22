using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Employee?> GetByUserIdAsync(int userId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task<Employee?> GetByEmployeeCodeAsync(string employeeCode)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<Employee?> GetEmployeeDetailAsync(int employeeId)
        {
            return await _context.Employees
                .Include(e => e.User)
                .Include(e => e.Manager)
                .Include(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                    .ThenInclude(ea => ea.Department)
                .Include(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                    .ThenInclude(ea => ea.Position)
                .Include(e => e.Contract)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<List<Employee>> GetEmployeesByManagerIdAsync(int managerId)
        {
            return await _context.Employees
                .Where(e => e.ManagerId == managerId)
                .ToListAsync();
        }

        public async Task<List<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId)
        {
            return await _context.EmployeeAssignments
                .Where(ea => ea.DepartmentId == departmentId && ea.EndDate == null)
                .Select(ea => ea.Employee)
                .ToListAsync();
        }

        public async Task<bool> IsEmployeeCodeExistsAsync(string employeeCode)
        {
            return await _context.Employees
                .AnyAsync(e => e.EmployeeCode == employeeCode);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.Employees
                .AnyAsync(e => e.Email == email);
        }

        public async Task<bool> IsCccdExistsAsync(string cccd)
        {
            return await _context.Employees
                .AnyAsync(e => e.Cccd == cccd);
        }

        public async Task<bool> IsUserLinkedToEmployeeAsync(int userId)
        {
            return await _context.Employees
                .AnyAsync(e => e.UserId == userId);
        }

        public async Task<bool> IsUserLinkedToEmployeeAsync(int userId, int excludeEmployeeId)
        {
            return await _context.Employees
                .AnyAsync(e =>
                    e.UserId == userId &&
                    e.EmployeeId != excludeEmployeeId);
        }

        public async Task<bool> IsActiveEmployeeAsync(int employeeId)
        {
            return await _context.Employees
                .AnyAsync(e =>
                    e.EmployeeId == employeeId &&
                    e.EmploymentStatus == "ACTIVE");
        }

        public async Task<bool> IsEmployeeUnderManagerAsync(int employeeId, int managerEmployeeId)
        {
            return await _context.EmployeeAssignments
                .AnyAsync(ea =>
                    ea.EmployeeId == employeeId &&
                    ea.EndDate == null &&
                    ea.Department.ManagerEmployeeId == managerEmployeeId);
        }

        public async Task<EmployeeAssignment?> GetCurrentAssignmentAsync(int employeeId)
        {
            return await _context.EmployeeAssignments
                .Include(ea => ea.Employee)
                .Include(ea => ea.Department)
                .Include(ea => ea.Position)
                .FirstOrDefaultAsync(ea =>
                    ea.EmployeeId == employeeId &&
                    ea.EndDate == null);
        }

        public async Task<bool> HasCurrentAssignmentAsync(int employeeId)
        {
            return await _context.EmployeeAssignments
                .AnyAsync(ea =>
                    ea.EmployeeId == employeeId &&
                    ea.EndDate == null);
        }

        public async Task AddAssignmentAsync(EmployeeAssignment assignment)
        {
            await _context.EmployeeAssignments.AddAsync(assignment);
        }

        public void UpdateAssignment(EmployeeAssignment assignment)
        {
            _context.EmployeeAssignments.Update(assignment);
        }

        public async Task<List<EmployeeAssignment>> GetAssignmentHistoryAsync(int employeeId)
        {
            return await _context.EmployeeAssignments
                .Include(ea => ea.Employee)
                .Include(ea => ea.Department)
                .Include(ea => ea.Position)
                .Where(ea => ea.EmployeeId == employeeId)
                .OrderByDescending(ea => ea.StartDate)
                .ThenByDescending(ea => ea.AssignmentId)
                .ToListAsync();
        }
    }
}
