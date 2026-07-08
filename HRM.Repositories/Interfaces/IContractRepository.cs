using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IContractRepository : IGenericRepository<Contract>
    {
        Task<Contract?> GetActiveContractByEmployeeIdAsync(int employeeId);

        Task<List<Contract>> GetContractsByEmployeeIdAsync(int employeeId);

        Task<List<Contract>> GetContractsExpiringSoonAsync(DateTime fromDate, DateTime toDate);

        Task<bool> HasActiveContractAsync(int employeeId);
    }
}