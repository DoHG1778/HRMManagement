using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class KpiRepository : GenericRepository<Kpi>, IKpiRepository
    {
        public KpiRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Kpi?> GetByNameAsync(string kpiName)
        {
            return await _context.Kpis
                .FirstOrDefaultAsync(k => k.Kpiname == kpiName);
        }

        public async Task<Kpi?> GetDetailAsync(int kpiId)
        {
            return await _context.Kpis
                .Include(k => k.CreatedByUser)
                .FirstOrDefaultAsync(k => k.Kpiid == kpiId);
        }

        public async Task<List<Kpi>> GetActiveKpisAsync()
        {
            return await _context.Kpis
                .Where(k => k.IsActive)
                .ToListAsync();
        }

        public async Task<List<Kpi>> GetAllWithCreatorAsync()
        {
            return await _context.Kpis
                .Include(k => k.CreatedByUser)
                .ToListAsync();
        }

        public async Task<bool> IsKpiNameExistsAsync(string kpiName)
        {
            return await _context.Kpis
                .AnyAsync(k => k.Kpiname == kpiName);
        }

        public async Task<bool> HasKpiAssignmentsAsync(int kpiId)
        {
            return await _context.Kpiassignments
                .AnyAsync(ka => ka.Kpiid == kpiId);
        }

        public async Task<List<Kpiassignment>> GetKpiAssignmentsByEmployeeAsync(int employeeId)
        {
            return await _context.Kpiassignments
                .Include(ka => ka.Kpi)
                .Where(ka => ka.EmployeeId == employeeId)
                .OrderByDescending(ka => ka.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Kpiassignment>> GetKpiAssignmentsByManagerAsync(int managerEmployeeId)
        {
            return await _context.Kpiassignments
                .Include(ka => ka.Kpi)
                .Include(ka => ka.Employee)
                .Where(ka =>
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == ka.EmployeeId &&
                        ea.EndDate == null &&
                        ea.Department.ManagerEmployeeId == managerEmployeeId))
                .OrderByDescending(ka => ka.CreatedAt)
                .ToListAsync();
        }

        public async Task<Kpiassignment?> GetKpiAssignmentDetailAsync(int assignmentId)
        {
            return await _context.Kpiassignments
                .Include(ka => ka.Kpi)
                .Include(ka => ka.Employee)
                .FirstOrDefaultAsync(ka => ka.AssignmentId == assignmentId);
        }

        public async Task<bool> IsKpiAssignmentOwnedByEmployeeAsync(int assignmentId, int employeeId)
        {
            return await _context.Kpiassignments
                .AnyAsync(ka =>
                    ka.AssignmentId == assignmentId &&
                    ka.EmployeeId == employeeId);
        }

        public async Task<bool> IsKpiAssignmentUnderManagerAsync(int assignmentId, int managerEmployeeId)
        {
            return await _context.Kpiassignments
                .AnyAsync(ka =>
                    ka.AssignmentId == assignmentId &&
                    _context.EmployeeAssignments.Any(ea =>
                        ea.EmployeeId == ka.EmployeeId &&
                        ea.EndDate == null &&
                        ea.Department.ManagerEmployeeId == managerEmployeeId));
        }

        public async Task AddAssignmentAsync(Kpiassignment assignment)
        {
            await _context.Kpiassignments.AddAsync(assignment);
        }

        public async Task<bool> HasAssignmentAsync(int kpiId, int employeeId)
        {
            return await _context.Kpiassignments.AnyAsync(x =>
                x.Kpiid == kpiId &&
                x.EmployeeId == employeeId);
        }

        public async Task<Kpiassignment?> GetAssignmentByIdAsync(int assignmentId)
        {
            return await _context.Kpiassignments
                .Include(x => x.Kpi)
                .Include(x => x.Employee)
                .Include(x => x.AssignedByNavigation)
                .Include(x => x.ReviewedByNavigation)
                .FirstOrDefaultAsync(x => x.AssignmentId == assignmentId);
        }

        public void UpdateAssignment(Kpiassignment assignment)
        {
            _context.Kpiassignments.Update(assignment);
        }

        public async Task<List<Kpiassignment>> GetAssignmentsByEmployeeAsync(int employeeId)
        {
            return await _context.Kpiassignments
                .Include(x => x.Kpi)
                .Include(x => x.Employee)
                .Include(x => x.AssignedByNavigation)
                .Include(x => x.ReviewedByNavigation)
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public IQueryable<Kpiassignment> QueryAssignments()
        {
            return _context.Kpiassignments.AsQueryable();
        }
    }
}