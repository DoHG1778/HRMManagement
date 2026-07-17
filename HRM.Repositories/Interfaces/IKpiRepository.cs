using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IKpiRepository : IGenericRepository<Kpi>
    {
        Task<Kpi?> GetByNameAsync(string kpiName);

        Task<Kpi?> GetDetailAsync(int kpiId);

        Task<List<Kpi>> GetActiveKpisAsync();

        Task<List<Kpi>> GetAllWithCreatorAsync();

        Task<bool> IsKpiNameExistsAsync(string kpiName);

        Task<bool> HasKpiAssignmentsAsync(int kpiId);

        Task<List<Kpiassignment>> GetKpiAssignmentsByEmployeeAsync(int employeeId);

        Task<List<Kpiassignment>> GetKpiAssignmentsByManagerAsync(int managerEmployeeId);

        Task<Kpiassignment?> GetKpiAssignmentDetailAsync(int assignmentId);

        Task<bool> IsKpiAssignmentOwnedByEmployeeAsync(int assignmentId, int employeeId);

        Task<bool> IsKpiAssignmentUnderManagerAsync(int assignmentId, int managerEmployeeId);

        Task AddAssignmentAsync(Kpiassignment assignment);

        Task<bool> HasAssignmentAsync(int kpiId, int employeeId);

        Task<Kpiassignment?> GetAssignmentByIdAsync(int assignmentId);

        void UpdateAssignment(Kpiassignment assignment);

        Task<List<Kpiassignment>> GetAssignmentsByEmployeeAsync(int employeeId);

        IQueryable<Kpiassignment> QueryAssignments();
    }
}