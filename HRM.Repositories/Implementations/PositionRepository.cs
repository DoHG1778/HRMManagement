using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class PositionRepository : GenericRepository<Position>, IPositionRepository
    {
        public PositionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Position?> GetByNameAsync(string positionName)
        {
            return await _context.Positions
                .FirstOrDefaultAsync(p => p.PositionName == positionName);
        }

        public async Task<List<Position>> GetActivePositionsAsync()
        {
            return await _context.Positions
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<bool> IsPositionNameExistsAsync(string positionName)
        {
            return await _context.Positions
                .AnyAsync(p => p.PositionName == positionName);
        }

        public async Task<bool> HasEmployeeAssignmentsAsync(int positionId)
        {
            return await _context.EmployeeAssignments
                .AnyAsync(ea => ea.PositionId == positionId);
        }
    }
}