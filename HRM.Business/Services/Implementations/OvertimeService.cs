using HRM.Business.Common;
using HRM.Business.DTOs.Overtimes;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class OvertimeService : IOvertimeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OvertimeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> CreateOvertimeRequestAsync(
            CurrentUser currentUser,
            CreateOvertimeRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Employee not found.");

            var employee = await _unitOfWork.Employees.GetByIdAsync(currentUser.EmployeeId.Value);
            if (employee == null)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Employee not found.");
            if (employee.EmploymentStatus != "ACTIVE")
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Employee is not active.");

            if (request.Otdate < DateOnly.FromDateTime(DateTime.Today))
                return ApiResponse<OvertimeRequestResponseDto>.Fail("OT date cannot be in the past.");
            if (request.StartTime < DateTime.Now)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Start time cannot be in the past.");
            if (request.EndTime <= request.StartTime)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("End time must be greater than start time.");

            if (request.StartTime.Date != request.Otdate.ToDateTime(TimeOnly.MinValue).Date)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Start time must be on the same date as OT date.");
            if (request.EndTime.Date != request.Otdate.ToDateTime(TimeOnly.MinValue).Date)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("End time must be on the same date as OT date.");

            var totalHours = (decimal)(request.EndTime - request.StartTime).TotalHours;
            if (totalHours <= 0)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Total hours must be greater than 0.");
            if (totalHours > 3)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Overtime cannot exceed 3 hours per request.");

            var hasOverlap = await _unitOfWork.OvertimeRequests.HasOverlappingOvertimeRequestAsync(
                currentUser.EmployeeId.Value,
                request.StartTime,
                request.EndTime);
            if (hasOverlap)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Overtime request overlaps with an existing PENDING or APPROVED request.");

            var entity = new Models.Entities.OvertimeRequest
            {
                EmployeeId = currentUser.EmployeeId.Value,
                Otdate = request.Otdate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                TotalHours = totalHours,
                Reason = request.Reason,
                Status = "PENDING",
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.OvertimeRequests.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.OvertimeRequests.Query()
                .Include(o => o.Employee)
                .FirstAsync(o => o.Otid == entity.Otid);

            return ApiResponse<OvertimeRequestResponseDto>.Ok(MapOvertimeToDto(created), "Overtime request created successfully.", 201);
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> UpdateOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId,
            UpdateOvertimeRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Employee not found.");

            var existing = await _unitOfWork.OvertimeRequests.Query()
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(o => o.Otid == overtimeRequestId);

            if (existing == null)
                return ApiResponse<OvertimeRequestResponseDto>.NotFound("Overtime request not found.");
            if (existing.EmployeeId != currentUser.EmployeeId.Value)
                return ApiResponse<OvertimeRequestResponseDto>.Forbidden("You can only update your own overtime requests.");
            if (existing.Status != "PENDING")
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Only PENDING requests can be updated.");

            if (request.Otdate < DateOnly.FromDateTime(DateTime.Today))
                return ApiResponse<OvertimeRequestResponseDto>.Fail("OT date cannot be in the past.");
            if (request.StartTime < DateTime.Now)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Start time cannot be in the past.");
            if (request.EndTime <= request.StartTime)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("End time must be greater than start time.");

            if (request.StartTime.Date != request.Otdate.ToDateTime(TimeOnly.MinValue).Date)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Start time must be on the same date as OT date.");

            var totalHours = (decimal)(request.EndTime - request.StartTime).TotalHours;
            if (totalHours <= 0)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Total hours must be greater than 0.");
            if (totalHours > 3)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Overtime cannot exceed 3 hours per request.");

            var hasOverlap = await _unitOfWork.OvertimeRequests.HasOverlappingOvertimeRequestAsync(
                currentUser.EmployeeId.Value, request.StartTime, request.EndTime, overtimeRequestId);
            if (hasOverlap)
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Overtime request overlaps with an existing PENDING or APPROVED request.");

            existing.Otdate = request.Otdate;
            existing.StartTime = request.StartTime;
            existing.EndTime = request.EndTime;
            existing.TotalHours = totalHours;
            existing.Reason = request.Reason;
            existing.UpdatedAt = DateTime.Now;

            _unitOfWork.OvertimeRequests.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<OvertimeRequestResponseDto>.Ok(MapOvertimeToDto(existing), "Overtime request updated successfully.");
        }

        public async Task<ApiResponse<bool>> CancelOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<bool>.Fail("Employee not found.");

            var existing = await _unitOfWork.OvertimeRequests.GetByIdAsync(overtimeRequestId);
            if (existing == null)
                return ApiResponse<bool>.NotFound("Overtime request not found.");
            if (existing.EmployeeId != currentUser.EmployeeId.Value)
                return ApiResponse<bool>.Forbidden("You can only cancel your own overtime requests.");
            if (existing.Status != "PENDING")
                return ApiResponse<bool>.Fail("Only PENDING requests can be cancelled.");

            existing.Status = "CANCELLED";
            existing.UpdatedAt = DateTime.Now;
            _unitOfWork.OvertimeRequests.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Overtime request cancelled successfully.");
        }

        public async Task<ApiResponse<PagedResult<OvertimeRequestResponseDto>>> GetMyOvertimeRequestsAsync(
            CurrentUser currentUser,
            OvertimeRequestFilterDto filter)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<PagedResult<OvertimeRequestResponseDto>>.Fail("Employee not found.");

            var requests = await _unitOfWork.OvertimeRequests.GetPersonalOvertimeRequestsAsync(
                currentUser.EmployeeId.Value, filter.Status, filter.Month, filter.Year);

            var query = requests.AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.Otdate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(o => o.Otdate <= filter.ToDate.Value);

            var totalItems = query.Count();
            var items = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(o => MapOvertimeToDto(o))
                .ToList();

            var paged = PagedResult<OvertimeRequestResponseDto>.Create(items, filter.PageNumber, filter.PageSize, totalItems);
            return ApiResponse<PagedResult<OvertimeRequestResponseDto>>.Ok(paged);
        }

        public async Task<ApiResponse<PagedResult<OvertimeRequestResponseDto>>> GetOvertimeRequestsAsync(
            CurrentUser currentUser,
            OvertimeRequestFilterDto filter)
        {
            var query = _unitOfWork.OvertimeRequests.Query()
                .Include(o => o.Employee)
                .AsQueryable();

            if (filter.EmployeeId.HasValue)
                query = query.Where(o => o.EmployeeId == filter.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(o => o.Status == filter.Status);
            if (filter.Month.HasValue)
                query = query.Where(o => o.Otdate.Month == filter.Month.Value);
            if (filter.Year.HasValue)
                query = query.Where(o => o.Otdate.Year == filter.Year.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(o => o.Otdate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(o => o.Otdate <= filter.ToDate.Value);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = items.Select(MapOvertimeToDto).ToList();
            var paged = PagedResult<OvertimeRequestResponseDto>.Create(dtos, filter.PageNumber, filter.PageSize, totalItems);
            return ApiResponse<PagedResult<OvertimeRequestResponseDto>>.Ok(paged);
        }

        public async Task<ApiResponse<List<OvertimeRequestResponseDto>>> GetPendingOvertimeRequestsAsync(
            CurrentUser currentUser)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<List<OvertimeRequestResponseDto>>.Fail("Employee not found.");

            var requests = await _unitOfWork.OvertimeRequests.GetPendingOvertimeRequestsByManagerAsync(currentUser.EmployeeId.Value);
            var dtos = requests.Select(MapOvertimeToDto).ToList();
            return ApiResponse<List<OvertimeRequestResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<OvertimeRequestResponseDto>> ApproveOrRejectOvertimeRequestAsync(
            CurrentUser currentUser,
            int overtimeRequestId,
            ApproveOvertimeRequestDto request)
        {
            var existing = await _unitOfWork.OvertimeRequests.Query()
                .Include(o => o.Employee)
                .FirstOrDefaultAsync(o => o.Otid == overtimeRequestId);

            if (existing == null)
                return ApiResponse<OvertimeRequestResponseDto>.NotFound("Overtime request not found.");
            if (existing.Status != "PENDING")
                return ApiResponse<OvertimeRequestResponseDto>.Fail("Only PENDING requests can be approved or rejected.");

            if (currentUser.EmployeeId.HasValue && currentUser.EmployeeId.Value == existing.EmployeeId)
            {
                return ApiResponse<OvertimeRequestResponseDto>.Forbidden("You cannot approve or reject your own overtime request.");
            }

            bool isAuthorized = currentUser.IsAdmin();
            if (!isAuthorized && currentUser.EmployeeId.HasValue)
            {
                isAuthorized = await _unitOfWork.OvertimeRequests.IsOvertimeRequestUnderManagerAsync(overtimeRequestId, currentUser.EmployeeId.Value);
            }

            if (!isAuthorized)
            {
                return ApiResponse<OvertimeRequestResponseDto>.Forbidden("You are not authorized to approve/reject this request.");
            }

            if (request.IsApproved)
            {
                existing.Status = "APPROVED";
                existing.ApprovedBy = currentUser.EmployeeId;
                existing.ApprovedAt = DateTime.Now;
            }
            else
            {
                existing.Status = "REJECTED";
                existing.RejectionReason = request.RejectionReason;
                existing.ApprovedBy = currentUser.EmployeeId;
                existing.ApprovedAt = DateTime.Now;
            }

            existing.UpdatedAt = DateTime.Now;
            _unitOfWork.OvertimeRequests.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<OvertimeRequestResponseDto>.Ok(MapOvertimeToDto(existing),
                request.IsApproved ? "Overtime request approved." : "Overtime request rejected.");
        }

        public async Task<ApiResponse<List<OvertimeRequestResponseDto>>> GetApprovedOvertimeForPayrollAsync(
            CurrentUser currentUser,
            int employeeId,
            int month,
            int year)
        {
            var requests = await _unitOfWork.OvertimeRequests.GetApprovedOvertimeForPayrollAsync(employeeId, month, year);
            var dtos = requests.Select(MapOvertimeToDto).ToList();
            return ApiResponse<List<OvertimeRequestResponseDto>>.Ok(dtos);
        }

        private static OvertimeRequestResponseDto MapOvertimeToDto(Models.Entities.OvertimeRequest entity)
        {
            return new OvertimeRequestResponseDto
            {
                Otid = entity.Otid,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = entity.Employee?.EmployeeCode,
                EmployeeName = entity.Employee?.FullName,
                Otdate = entity.Otdate,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                TotalHours = entity.TotalHours,
                Reason = entity.Reason,
                Status = entity.Status,
                ApprovedBy = entity.ApprovedBy,
                ApprovedByName = entity.ApprovedByNavigation?.FullName,
                ApprovedAt = entity.ApprovedAt,
                RejectionReason = entity.RejectionReason,
                IsTransferredToPayroll = entity.IsTransferredToPayroll,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
