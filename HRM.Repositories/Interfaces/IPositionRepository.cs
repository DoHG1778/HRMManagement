using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface IPositionRepository : IGenericRepository<Position>
    {
        Task<Position?> GetByNameAsync(string positionName);

        Task<List<Position>> GetActivePositionsAsync();

        Task<bool> IsPositionNameExistsAsync(string positionName);

        Task<bool> HasEmployeeAssignmentsAsync(int positionId);
    }
}