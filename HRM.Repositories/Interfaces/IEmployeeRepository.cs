using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<Employee?> GetByUserIdAsync(int userId);

        Task<Employee?> GetByEmployeeCodeAsync(string employeeCode);

        Task<Employee?> GetByEmailAsync(string email);

        Task<Employee?> GetEmployeeDetailAsync(int employeeId);

        Task<List<Employee>> GetEmployeesByManagerIdAsync(int managerId);

        Task<List<Employee>> GetEmployeesByDepartmentIdAsync(int departmentId);

        Task<bool> IsEmployeeCodeExistsAsync(string employeeCode);

        Task<bool> IsEmailExistsAsync(string email);

        Task<bool> IsCccdExistsAsync(string cccd);

        Task<bool> IsUserLinkedToEmployeeAsync(int userId);

        Task<bool> IsUserLinkedToEmployeeAsync(int userId, int excludeEmployeeId);

        Task<bool> IsActiveEmployeeAsync(int employeeId);

        Task<bool> IsEmployeeUnderManagerAsync(int employeeId, int managerEmployeeId);

        Task<EmployeeAssignment?> GetCurrentAssignmentAsync(int employeeId);

        Task<bool> HasCurrentAssignmentAsync(int employeeId);

        Task AddAssignmentAsync(EmployeeAssignment assignment);

        void UpdateAssignment(EmployeeAssignment assignment);

        Task<List<EmployeeAssignment>> GetAssignmentHistoryAsync(int employeeId);
    }
}
