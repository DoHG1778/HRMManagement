using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Department?> GetByNameAsync(string departmentName)
        {
            return await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentName == departmentName);
        }

        public async Task<Department?> GetDepartmentDetailAsync(int departmentId)
        {
            return await _context.Departments
                .Include(d => d.ManagerEmployee)
                .Include(d => d.EmployeeAssignments)
                    .ThenInclude(ea => ea.Employee)
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
        }

        public async Task<List<Department>> GetActiveDepartmentsAsync()
        {
            return await _context.Departments
                .Where(d => d.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsDepartmentNameExistsAsync(string departmentName)
        {
            return await _context.Departments
                .AnyAsync(d => d.DepartmentName == departmentName);
        }

        public async Task<bool> HasActiveEmployeesAsync(int departmentId)
        {
            return await _context.EmployeeAssignments
                .AnyAsync(ea =>
                    ea.DepartmentId == departmentId &&
                    ea.EndDate == null &&
                    ea.Employee.EmploymentStatus == "ACTIVE");
        }
    }
}