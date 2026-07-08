using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        Task<Department?> GetByNameAsync(string departmentName);

        Task<Department?> GetDepartmentDetailAsync(int departmentId);

        Task<List<Department>> GetActiveDepartmentsAsync();

        Task<bool> IsDepartmentNameExistsAsync(string departmentName);

        Task<bool> HasActiveEmployeesAsync(int departmentId);
    }
}