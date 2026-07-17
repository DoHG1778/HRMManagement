using HRM.Business.Common;
using HRM.Business.DTOs.Kpis;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;
using HRM.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class KpiService : IKpiService
    {
        private readonly IUnitOfWork _unitOfWork;

        public KpiService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<KpiResponseDto>>> GetKpisAsync(bool? isActive = null)
        {
            List<Kpi> kpis;

            if (isActive.HasValue)
            {
                kpis = await _unitOfWork.Kpis
                    .Query()
                    .Include(k => k.CreatedByUser)
                    .Where(k => k.IsActive == isActive.Value)
                    .ToListAsync();
            }
            else
            {
                kpis = await _unitOfWork.Kpis.GetAllWithCreatorAsync();
            }

            var response = kpis.Select(k => new KpiResponseDto
            {
                Kpiid = k.Kpiid,
                Kpiname = k.Kpiname,
                Description = k.Description,
                Weight = k.Weight,
                IsActive = k.IsActive,
                CreatedByUserId = k.CreatedByUserId,
                CreatedByUsername = k.CreatedByUser?.Username,
                CreatedAt = k.CreatedAt,
                UpdatedAt = k.UpdatedAt
            }).ToList();

            return ApiResponse<List<KpiResponseDto>>.Ok(response);
        }

        public async Task<ApiResponse<KpiResponseDto>> GetKpiByIdAsync(int kpiId)
        {
            var kpi = await _unitOfWork.Kpis.GetDetailAsync(kpiId);

            if (kpi == null)
            {
                return ApiResponse<KpiResponseDto>.NotFound("KPI not found.");
            }

            var response = new KpiResponseDto
            {
                Kpiid = kpi.Kpiid,
                Kpiname = kpi.Kpiname,
                Description = kpi.Description,
                Weight = kpi.Weight,
                IsActive = kpi.IsActive,
                CreatedByUserId = kpi.CreatedByUserId,
                CreatedByUsername = kpi.CreatedByUser?.Username,
                CreatedAt = kpi.CreatedAt,
                UpdatedAt = kpi.UpdatedAt
            };

            return ApiResponse<KpiResponseDto>.Ok(response);
        }

        public async Task<ApiResponse<KpiResponseDto>> CreateKpiAsync(
            CurrentUser currentUser,
            CreateKpiRequestDto request)
        {
            if (!currentUser.IsHr() && !currentUser.IsManager())
            {
                return ApiResponse<KpiResponseDto>.Forbidden(
                    "Only HR or Manager can create KPI.");
            }

            var existed = await _unitOfWork.Kpis.IsKpiNameExistsAsync(request.Kpiname);

            if (existed)
            {
                return ApiResponse<KpiResponseDto>.Fail(
                    "KPI name already exists.");
            }

            var entity = new Kpi
            {
                Kpiname = request.Kpiname.Trim(),
                Description = request.Description,
                Weight = request.Weight,
                IsActive = true,
                CreatedByUserId = currentUser.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Kpis.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var response = new KpiResponseDto
            {
                Kpiid = entity.Kpiid,
                Kpiname = entity.Kpiname,
                Description = entity.Description,
                Weight = entity.Weight,
                IsActive = entity.IsActive,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedByUsername = currentUser.Username,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            return ApiResponse<KpiResponseDto>.Ok(
                response,
                "KPI created successfully.",
                201);
        }

        public async Task<ApiResponse<KpiResponseDto>> UpdateKpiAsync(
    CurrentUser currentUser,
    int kpiId,
    UpdateKpiRequestDto request)
        {
            if (!currentUser.IsHr() && !currentUser.IsManager())
            {
                return ApiResponse<KpiResponseDto>.Forbidden(
                    "Only HR or Manager can update KPI.");
            }

            var entity = await _unitOfWork.Kpis.GetDetailAsync(kpiId);

            if (entity == null)
            {
                return ApiResponse<KpiResponseDto>.NotFound("KPI not found.");
            }

            if (!string.IsNullOrWhiteSpace(request.Kpiname)
                && request.Kpiname.Trim() != entity.Kpiname)
            {
                var existed = await _unitOfWork.Kpis
                    .IsKpiNameExistsAsync(request.Kpiname.Trim());

                if (existed)
                {
                    return ApiResponse<KpiResponseDto>.Fail(
                        "KPI name already exists.");
                }

                entity.Kpiname = request.Kpiname.Trim();
            }

            if (request.Description != null)
            {
                entity.Description = request.Description;
            }

            if (request.Weight.HasValue)
            {
                entity.Weight = request.Weight.Value;
            }

            if (request.IsActive.HasValue)
            {
                entity.IsActive = request.IsActive.Value;
            }

            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Kpis.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            var response = new KpiResponseDto
            {
                Kpiid = entity.Kpiid,
                Kpiname = entity.Kpiname,
                Description = entity.Description,
                Weight = entity.Weight,
                IsActive = entity.IsActive,
                CreatedByUserId = entity.CreatedByUserId,
                CreatedByUsername = entity.CreatedByUser?.Username,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            return ApiResponse<KpiResponseDto>.Ok(
                response,
                "KPI updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeactivateKpiAsync(
            CurrentUser currentUser,
            int kpiId)
        {
            if (!currentUser.IsHr() && !currentUser.IsManager())
            {
                return ApiResponse<bool>.Forbidden(
                    "Only HR or Manager can deactivate KPI.");
            }

            var entity = await _unitOfWork.Kpis.GetByIdAsync(kpiId);

            if (entity == null)
            {
                return ApiResponse<bool>.NotFound("KPI not found.");
            }

            if (!entity.IsActive)
            {
                return ApiResponse<bool>.Fail("KPI is already inactive.");
            }

            entity.IsActive = false;
            entity.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Kpis.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(
                true,
                "KPI deactivated successfully.");
        }

        public async Task<ApiResponse<KpiAssignmentResponseDto>> AssignKpiAsync(
    CurrentUser currentUser,
    AssignKpiRequestDto request)
        {
            if (!currentUser.IsManager())
            {
                return ApiResponse<KpiAssignmentResponseDto>.Forbidden(
                    "Only Manager can assign KPI.");
            }

            if (currentUser.EmployeeId == null)
            {
                return ApiResponse<KpiAssignmentResponseDto>.Fail(
                    "Current user is not linked to an employee.");
            }

            var kpi = await _unitOfWork.Kpis.GetByIdAsync(request.Kpiid);

            if (kpi == null)
            {
                return ApiResponse<KpiAssignmentResponseDto>.NotFound(
                    "KPI not found.");
            }

            if (!kpi.IsActive)
            {
                return ApiResponse<KpiAssignmentResponseDto>.Fail(
                    "KPI is inactive.");
            }

            var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId);

            if (employee == null)
            {
                return ApiResponse<KpiAssignmentResponseDto>.NotFound(
                    "Employee not found.");
            }

            var assigned = await _unitOfWork.Kpis.HasAssignmentAsync(
                request.Kpiid,
                request.EmployeeId);

            if (assigned)
            {
                return ApiResponse<KpiAssignmentResponseDto>.Fail(
                    "This employee has already been assigned this KPI.");
            }

            var assignment = new Kpiassignment
            {
                Kpiid = request.Kpiid,
                EmployeeId = request.EmployeeId,
                AssignedBy = currentUser.EmployeeId.Value,
                TargetValue = request.TargetValue,
                ActualValue = 0,
                ProgressPercent = 0,
                EmployeeSelfScore = null,
                EmployeeComment = null,
                ManagerScore = null,
                ManagerComment = null,
                ReviewedBy = null,
                ReviewedAt = null,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = "Assigned",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Kpis.AddAssignmentAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.Kpis.GetAssignmentByIdAsync(assignment.AssignmentId);

            if (result == null)
            {
                return ApiResponse<KpiAssignmentResponseDto>.Fail(
                    "Assignment created but could not be loaded.");
            }

            var response = new KpiAssignmentResponseDto
            {
                AssignmentId = result.AssignmentId,
                Kpiid = result.Kpiid,
                Kpiname = result.Kpi?.Kpiname,
                EmployeeId = result.EmployeeId,
                EmployeeCode = result.Employee?.EmployeeCode,
                EmployeeName = result.Employee?.FullName,
                AssignedBy = result.AssignedBy,
                AssignedByName = result.AssignedByNavigation?.FullName,
                TargetValue = result.TargetValue,
                ActualValue = result.ActualValue,
                ProgressPercent = result.ProgressPercent,
                EmployeeComment = result.EmployeeComment,
                EmployeeSelfScore = result.EmployeeSelfScore,
                ManagerScore = result.ManagerScore,
                ManagerComment = result.ManagerComment,
                ReviewedBy = result.ReviewedBy,
                ReviewedByName = result.ReviewedByNavigation?.FullName,
                ReviewedAt = result.ReviewedAt,
                StartDate = result.StartDate,
                EndDate = result.EndDate,
                Status = result.Status,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };

            return ApiResponse<KpiAssignmentResponseDto>.Ok(
                response,
                "KPI assigned successfully.",
                201);
        }

        public async Task<ApiResponse<KpiAssignmentResponseDto>> UpdateKpiProgressAsync(
    CurrentUser currentUser,
    int assignmentId,
    UpdateKpiProgressRequestDto request)
        {
            if (!currentUser.IsEmployee())
            {
                return ApiResponse<KpiAssignmentResponseDto>.Forbidden(
                    "Only Employee can update KPI progress.");
            }

            if (currentUser.EmployeeId == null)
            {
                return ApiResponse<KpiAssignmentResponseDto>.Fail(
                    "Current user is not linked to an employee.");
            }

            var isOwner = await _unitOfWork.Kpis
                .IsKpiAssignmentOwnedByEmployeeAsync(
                    assignmentId,
                    currentUser.EmployeeId.Value);

            if (!isOwner)
            {
                return ApiResponse<KpiAssignmentResponseDto>.Forbidden(
                    "You can only update your own KPI.");
            }

            var assignment = await _unitOfWork.Kpis
                .GetAssignmentByIdAsync(assignmentId);

            if (assignment == null)
            {
                return ApiResponse<KpiAssignmentResponseDto>.NotFound(
                    "KPI assignment not found.");
            }

            assignment.ActualValue = request.ActualValue;
            assignment.ProgressPercent = request.ProgressPercent;
            assignment.EmployeeComment = request.EmployeeComment;
            assignment.EmployeeSelfScore = request.EmployeeSelfScore;
            assignment.UpdatedAt = DateTime.UtcNow;

            if (request.ProgressPercent >= 100)
            {
                assignment.Status = "COMPLETED";
            }
            else
            {
                assignment.Status = "IN_PROGRESS";
            }

            _unitOfWork.Kpis.UpdateAssignment(assignment);

            await _unitOfWork.SaveChangesAsync();
            var response = new KpiAssignmentResponseDto
            {
                AssignmentId = assignment.AssignmentId,
                Kpiid = assignment.Kpiid,
                Kpiname = assignment.Kpi?.Kpiname,
                EmployeeId = assignment.EmployeeId,
                EmployeeCode = assignment.Employee?.EmployeeCode,
                EmployeeName = assignment.Employee?.FullName,
                AssignedBy = assignment.AssignedBy,
                AssignedByName = assignment.AssignedByNavigation?.FullName,
                TargetValue = assignment.TargetValue,
                ActualValue = assignment.ActualValue,
                ProgressPercent = assignment.ProgressPercent,
                EmployeeComment = assignment.EmployeeComment,
                EmployeeSelfScore = assignment.EmployeeSelfScore,
                ManagerScore = assignment.ManagerScore,
                ManagerComment = assignment.ManagerComment,
                ReviewedBy = assignment.ReviewedBy,
                ReviewedByName = assignment.ReviewedByNavigation?.FullName,
                ReviewedAt = assignment.ReviewedAt,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                Status = assignment.Status,
                CreatedAt = assignment.CreatedAt,
                UpdatedAt = assignment.UpdatedAt
            };

            return ApiResponse<KpiAssignmentResponseDto>.Ok(
                response,
                "KPI progress updated successfully.");
        }

        public async Task<ApiResponse<KpiAssignmentResponseDto>> EvaluateKpiAsync(
    CurrentUser currentUser,
    int assignmentId,
    EvaluateKpiRequestDto request)
        {
            if (!currentUser.IsManager())
            {
                return ApiResponse<KpiAssignmentResponseDto>.Forbidden(
                    "Only Manager can evaluate KPI.");
            }

            if (currentUser.EmployeeId == null)
            {
                return ApiResponse<KpiAssignmentResponseDto>.Fail(
                    "Current user is not linked to an employee.");
            }

            var isManager = await _unitOfWork.Kpis
                .IsKpiAssignmentUnderManagerAsync(
                    assignmentId,
                    currentUser.EmployeeId.Value);

            if (!isManager)
            {
                return ApiResponse<KpiAssignmentResponseDto>.Forbidden(
                    "You can only evaluate KPIs of employees under your management.");
            }

            var assignment = await _unitOfWork.Kpis
                .GetAssignmentByIdAsync(assignmentId);

            if (assignment == null)
            {
                return ApiResponse<KpiAssignmentResponseDto>.NotFound(
                    "KPI assignment not found.");
            }

            assignment.ManagerScore = request.ManagerScore;
            assignment.ManagerComment = request.ManagerComment;
            assignment.ReviewedBy = currentUser.EmployeeId.Value;
            assignment.ReviewedAt = DateTime.UtcNow;

            if (assignment.ProgressPercent >= 100)
            {
                assignment.Status = "COMPLETED";
            }

            assignment.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Kpis.UpdateAssignment(assignment);

            await _unitOfWork.SaveChangesAsync();
            var response = new KpiAssignmentResponseDto
            {
                AssignmentId = assignment.AssignmentId,
                Kpiid = assignment.Kpiid,
                Kpiname = assignment.Kpi?.Kpiname,
                EmployeeId = assignment.EmployeeId,
                EmployeeCode = assignment.Employee?.EmployeeCode,
                EmployeeName = assignment.Employee?.FullName,
                AssignedBy = assignment.AssignedBy,
                AssignedByName = assignment.AssignedByNavigation?.FullName,
                TargetValue = assignment.TargetValue,
                ActualValue = assignment.ActualValue,
                ProgressPercent = assignment.ProgressPercent,
                EmployeeComment = assignment.EmployeeComment,
                EmployeeSelfScore = assignment.EmployeeSelfScore,
                ManagerScore = assignment.ManagerScore,
                ManagerComment = assignment.ManagerComment,
                ReviewedBy = assignment.ReviewedBy,
                ReviewedByName = assignment.ReviewedByNavigation?.FullName,
                ReviewedAt = assignment.ReviewedAt,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                Status = assignment.Status,
                CreatedAt = assignment.CreatedAt,
                UpdatedAt = assignment.UpdatedAt
            };

            return ApiResponse<KpiAssignmentResponseDto>.Ok(
                response,
                "KPI evaluated successfully.");
        }

        public async Task<ApiResponse<PagedResult<KpiAssignmentResponseDto>>> GetMyKpiAssignmentsAsync(
            CurrentUser currentUser,
            KpiAssignmentFilterDto filter)
        {
            if (currentUser.EmployeeId == null)
            {
                return ApiResponse<PagedResult<KpiAssignmentResponseDto>>
                    .Fail("Current user is not linked to an employee.");
            }

            var assignments = await _unitOfWork.Kpis
                .GetKpiAssignmentsByEmployeeAsync(currentUser.EmployeeId.Value);
            if (filter.Kpiid.HasValue)
            {
                assignments = assignments
                    .Where(x => x.Kpiid == filter.Kpiid.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                assignments = assignments
                    .Where(x => x.Status == filter.Status)
                    .ToList();
            }

            if (filter.FromDate.HasValue)
            {
                assignments = assignments
                    .Where(x => x.StartDate >= filter.FromDate)
                    .ToList();
            }

            if (filter.ToDate.HasValue)
            {
                assignments = assignments
                    .Where(x => x.EndDate <= filter.ToDate)
                    .ToList();
            }
            var result = assignments
            .Select(x => new KpiAssignmentResponseDto
            {
                AssignmentId = x.AssignmentId,
                Kpiid = x.Kpiid,
                Kpiname = x.Kpi?.Kpiname,
                EmployeeId = x.EmployeeId,
                EmployeeCode = x.Employee?.EmployeeCode,
                EmployeeName = x.Employee?.FullName,
                AssignedBy = x.AssignedBy,
                AssignedByName = x.AssignedByNavigation?.FullName,
                TargetValue = x.TargetValue,
                ActualValue = x.ActualValue,
                ProgressPercent = x.ProgressPercent,
                EmployeeComment = x.EmployeeComment,
                EmployeeSelfScore = x.EmployeeSelfScore,
                ManagerScore = x.ManagerScore,
                ManagerComment = x.ManagerComment,
                ReviewedBy = x.ReviewedBy,
                ReviewedByName = x.ReviewedByNavigation?.FullName,
                ReviewedAt = x.ReviewedAt,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
        .ToList();
            var totalItems = result.Count;

            var items = result
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var paged = PagedResult<KpiAssignmentResponseDto>.Create(
                items,
                filter.PageNumber,
                filter.PageSize,
                totalItems);

            return ApiResponse<PagedResult<KpiAssignmentResponseDto>>
                .Ok(paged);
        }


        public async Task<ApiResponse<PagedResult<KpiAssignmentResponseDto>>> GetKpiAssignmentsAsync(
            CurrentUser currentUser,
            KpiAssignmentFilterDto filter)
        {
            List<Kpiassignment> assignments;

            if (currentUser.IsAdmin() || currentUser.IsHr())
            {
                assignments = await _unitOfWork.Kpis.Query()
                    .SelectMany(k => k.Kpiassignments)
                    .ToListAsync();
            }
            else if (currentUser.IsManager())
            {
                if (currentUser.EmployeeId == null)
                {
                    return ApiResponse<PagedResult<KpiAssignmentResponseDto>>
                        .Fail("Current user is not linked to an employee.");
                }

                assignments = await _unitOfWork.Kpis
                    .GetKpiAssignmentsByManagerAsync(currentUser.EmployeeId.Value);
            }
            else
            {
                return ApiResponse<PagedResult<KpiAssignmentResponseDto>>
                    .Forbidden();
            }
            if (filter.EmployeeId.HasValue)
            {
                assignments = assignments
                    .Where(x => x.EmployeeId == filter.EmployeeId.Value)
                    .ToList();
            }

            if (filter.Kpiid.HasValue)
            {
                assignments = assignments
                    .Where(x => x.Kpiid == filter.Kpiid.Value)
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                assignments = assignments
                    .Where(x => x.Status == filter.Status)
                    .ToList();
            }
            var result = assignments
            .Select(x => new KpiAssignmentResponseDto
            {
                AssignmentId = x.AssignmentId,
                Kpiid = x.Kpiid,
                Kpiname = x.Kpi?.Kpiname,
                EmployeeId = x.EmployeeId,
                EmployeeCode = x.Employee?.EmployeeCode,
                EmployeeName = x.Employee?.FullName,
                AssignedBy = x.AssignedBy,
                AssignedByName = x.AssignedByNavigation?.FullName,
                TargetValue = x.TargetValue,
                ActualValue = x.ActualValue,
                ProgressPercent = x.ProgressPercent,
                EmployeeComment = x.EmployeeComment,
                EmployeeSelfScore = x.EmployeeSelfScore,
                ManagerScore = x.ManagerScore,
                ManagerComment = x.ManagerComment,
                ReviewedBy = x.ReviewedBy,
                ReviewedByName = x.ReviewedByNavigation?.FullName,
                ReviewedAt = x.ReviewedAt,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Status = x.Status,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
        .ToList();
            var totalItems = result.Count;

            var items = result
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var paged = PagedResult<KpiAssignmentResponseDto>.Create(
                items,
                filter.PageNumber,
                filter.PageSize,
                totalItems);

            return ApiResponse<PagedResult<KpiAssignmentResponseDto>>
                .Ok(paged);
        }
    }
}