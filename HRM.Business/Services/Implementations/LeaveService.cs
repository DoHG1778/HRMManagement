using HRM.Business.Common;
using HRM.Business.DTOs.Leaves;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class LeaveService : ILeaveService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<LeaveType> _leaveTypeRepo;
        private readonly IGenericRepository<LeaveBalance> _leaveBalanceRepo;

        public LeaveService(
            IUnitOfWork unitOfWork,
            IGenericRepository<LeaveType> leaveTypeRepo,
            IGenericRepository<LeaveBalance> leaveBalanceRepo)
        {
            _unitOfWork = unitOfWork;
            _leaveTypeRepo = leaveTypeRepo;
            _leaveBalanceRepo = leaveBalanceRepo;
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> CreateLeaveRequestAsync(
            CurrentUser currentUser,
            CreateLeaveRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Employee not found.");

            var employee = await _unitOfWork.Employees.GetByIdAsync(currentUser.EmployeeId.Value);
            if (employee == null)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Employee not found.");
            if (employee.EmploymentStatus != "ACTIVE")
                return ApiResponse<LeaveRequestResponseDto>.Fail("Employee is not active.");

            var leaveType = await _leaveTypeRepo.GetByIdAsync(request.LeaveTypeId);
            if (leaveType == null)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Leave type not found.");
            if (!leaveType.IsActive)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Leave type is not active.");

            if (request.StartDate < DateOnly.FromDateTime(DateTime.Today))
                return ApiResponse<LeaveRequestResponseDto>.Fail("Start date cannot be in the past.");
            if (request.EndDate < DateOnly.FromDateTime(DateTime.Today))
                return ApiResponse<LeaveRequestResponseDto>.Fail("End date cannot be in the past.");
            if (request.EndDate < request.StartDate)
                return ApiResponse<LeaveRequestResponseDto>.Fail("End date must be greater than or equal to start date.");

            int totalDays = (request.EndDate.DayNumber - request.StartDate.DayNumber) + 1;

            var hasOverlap = await _unitOfWork.LeaveRequests.HasOverlappingLeaveRequestAsync(
                currentUser.EmployeeId.Value,
                request.StartDate.ToDateTime(TimeOnly.MinValue),
                request.EndDate.ToDateTime(TimeOnly.MaxValue));
            if (hasOverlap)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Leave request overlaps with an existing PENDING or APPROVED request.");

            var currentYear = request.StartDate.Year;
            var balance = await _leaveBalanceRepo.FirstOrDefaultAsync(b =>
                b.EmployeeId == currentUser.EmployeeId.Value &&
                b.LeaveTypeId == request.LeaveTypeId &&
                b.Year == currentYear);

            int usedDays = balance?.UsedDays ?? 0;
            if (usedDays + totalDays > leaveType.MaxDaysPerYear)
                return ApiResponse<LeaveRequestResponseDto>.Fail(
                    $"Insufficient leave balance. You have {leaveType.MaxDaysPerYear - usedDays} days remaining for {leaveType.LeaveTypeName}.");

            var entity = new LeaveRequest
            {
                EmployeeId = currentUser.EmployeeId.Value,
                LeaveTypeId = request.LeaveTypeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TotalDays = totalDays,
                Reason = request.Reason,
                Status = "PENDING",
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.LeaveRequests.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.LeaveRequests.Query()
                .Include(l => l.LeaveType)
                .Include(l => l.Employee)
                .FirstAsync(l => l.LeaveRequestId == entity.LeaveRequestId);

            return ApiResponse<LeaveRequestResponseDto>.Ok(MapLeaveRequestToDto(created), "Leave request created successfully.", 201);
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> UpdateLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId,
            UpdateLeaveRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Employee not found.");

            var existing = await _unitOfWork.LeaveRequests.GetByIdAsync(leaveRequestId);
            if (existing == null)
                return ApiResponse<LeaveRequestResponseDto>.NotFound("Leave request not found.");
            if (existing.EmployeeId != currentUser.EmployeeId.Value)
                return ApiResponse<LeaveRequestResponseDto>.Forbidden("You can only update your own leave requests.");
            if (existing.Status != "PENDING")
                return ApiResponse<LeaveRequestResponseDto>.Fail("Only PENDING requests can be updated.");

            var leaveType = await _leaveTypeRepo.GetByIdAsync(request.LeaveTypeId);
            if (leaveType == null)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Leave type not found.");
            if (!leaveType.IsActive)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Leave type is not active.");

            if (request.StartDate < DateOnly.FromDateTime(DateTime.Today))
                return ApiResponse<LeaveRequestResponseDto>.Fail("Start date cannot be in the past.");
            if (request.EndDate < DateOnly.FromDateTime(DateTime.Today))
                return ApiResponse<LeaveRequestResponseDto>.Fail("End date cannot be in the past.");
            if (request.EndDate < request.StartDate)
                return ApiResponse<LeaveRequestResponseDto>.Fail("End date must be greater than or equal to start date.");

            int totalDays = (request.EndDate.DayNumber - request.StartDate.DayNumber) + 1;

            var hasOverlap = await _unitOfWork.LeaveRequests.HasOverlappingLeaveRequestAsync(
                currentUser.EmployeeId.Value,
                request.StartDate.ToDateTime(TimeOnly.MinValue),
                request.EndDate.ToDateTime(TimeOnly.MaxValue),
                leaveRequestId);
            if (hasOverlap)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Leave request overlaps with an existing PENDING or APPROVED request.");

            var currentYear = request.StartDate.Year;
            var balance = await _leaveBalanceRepo.FirstOrDefaultAsync(b =>
                b.EmployeeId == currentUser.EmployeeId.Value &&
                b.LeaveTypeId == request.LeaveTypeId &&
                b.Year == currentYear);

            int usedDays = balance?.UsedDays ?? 0;
            if (usedDays + totalDays > leaveType.MaxDaysPerYear)
                return ApiResponse<LeaveRequestResponseDto>.Fail(
                    $"Insufficient leave balance. You have {leaveType.MaxDaysPerYear - usedDays} days remaining for {leaveType.LeaveTypeName}.");

            existing.LeaveTypeId = request.LeaveTypeId;
            existing.StartDate = request.StartDate;
            existing.EndDate = request.EndDate;
            existing.TotalDays = totalDays;
            existing.Reason = request.Reason;
            existing.UpdatedAt = DateTime.Now;

            _unitOfWork.LeaveRequests.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.LeaveRequests.Query()
                .Include(l => l.LeaveType)
                .Include(l => l.Employee)
                .FirstAsync(l => l.LeaveRequestId == leaveRequestId);

            return ApiResponse<LeaveRequestResponseDto>.Ok(MapLeaveRequestToDto(updated), "Leave request updated successfully.");
        }

        public async Task<ApiResponse<bool>> CancelLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<bool>.Fail("Employee not found.");

            var existing = await _unitOfWork.LeaveRequests.GetByIdAsync(leaveRequestId);
            if (existing == null)
                return ApiResponse<bool>.NotFound("Leave request not found.");
            if (existing.EmployeeId != currentUser.EmployeeId.Value)
                return ApiResponse<bool>.Forbidden("You can only cancel your own leave requests.");
            if (existing.Status != "PENDING")
                return ApiResponse<bool>.Fail("Only PENDING requests can be cancelled.");

            existing.Status = "CANCELLED";
            existing.UpdatedAt = DateTime.Now;
            _unitOfWork.LeaveRequests.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Leave request cancelled successfully.");
        }

        public async Task<ApiResponse<PagedResult<LeaveRequestResponseDto>>> GetMyLeaveRequestsAsync(
            CurrentUser currentUser,
            LeaveRequestFilterDto filter)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<PagedResult<LeaveRequestResponseDto>>.Fail("Employee not found.");

            var requests = await _unitOfWork.LeaveRequests.GetPersonalLeaveRequestsAsync(
                currentUser.EmployeeId.Value, filter.Status, filter.Month, filter.Year);

            var query = requests.AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(l => l.StartDate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(l => l.EndDate <= filter.ToDate.Value);
            if (filter.LeaveTypeId.HasValue)
                query = query.Where(l => l.LeaveTypeId == filter.LeaveTypeId.Value);

            var totalItems = query.Count();
            var items = query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(MapLeaveRequestToDto)
                .ToList();

            var paged = PagedResult<LeaveRequestResponseDto>.Create(items, filter.PageNumber, filter.PageSize, totalItems);
            return ApiResponse<PagedResult<LeaveRequestResponseDto>>.Ok(paged);
        }

        public async Task<ApiResponse<PagedResult<LeaveRequestResponseDto>>> GetLeaveRequestsAsync(
            CurrentUser currentUser,
            LeaveRequestFilterDto filter)
        {
            var query = _unitOfWork.LeaveRequests.Query()
                .Include(l => l.LeaveType)
                .Include(l => l.Employee)
                .AsQueryable();

            if (filter.EmployeeId.HasValue)
                query = query.Where(l => l.EmployeeId == filter.EmployeeId.Value);
            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(l => l.Status == filter.Status);
            if (filter.LeaveTypeId.HasValue)
                query = query.Where(l => l.LeaveTypeId == filter.LeaveTypeId.Value);
            if (filter.Month.HasValue)
                query = query.Where(l => l.StartDate.Month == filter.Month.Value);
            if (filter.Year.HasValue)
                query = query.Where(l => l.StartDate.Year == filter.Year.Value);
            if (filter.FromDate.HasValue)
                query = query.Where(l => l.StartDate >= filter.FromDate.Value);
            if (filter.ToDate.HasValue)
                query = query.Where(l => l.EndDate <= filter.ToDate.Value);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = items.Select(MapLeaveRequestToDto).ToList();
            var paged = PagedResult<LeaveRequestResponseDto>.Create(dtos, filter.PageNumber, filter.PageSize, totalItems);
            return ApiResponse<PagedResult<LeaveRequestResponseDto>>.Ok(paged);
        }

        public async Task<ApiResponse<List<LeaveRequestResponseDto>>> GetPendingLeaveRequestsAsync(
            CurrentUser currentUser)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<List<LeaveRequestResponseDto>>.Fail("Employee not found.");

            var requests = await _unitOfWork.LeaveRequests.GetPendingLeaveRequestsByManagerAsync(currentUser.EmployeeId.Value);
            var dtos = requests.Select(MapLeaveRequestToDto).ToList();
            return ApiResponse<List<LeaveRequestResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<LeaveRequestResponseDto>> ApproveOrRejectLeaveRequestAsync(
            CurrentUser currentUser,
            int leaveRequestId,
            ApproveLeaveRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<LeaveRequestResponseDto>.Fail("Employee not found.");

            var existing = await _unitOfWork.LeaveRequests.Query()
                .Include(l => l.LeaveType)
                .Include(l => l.Employee)
                .FirstOrDefaultAsync(l => l.LeaveRequestId == leaveRequestId);

            if (existing == null)
                return ApiResponse<LeaveRequestResponseDto>.NotFound("Leave request not found.");
            if (existing.Status != "PENDING")
                return ApiResponse<LeaveRequestResponseDto>.Fail("Only PENDING requests can be approved or rejected.");

            var underManager = await _unitOfWork.LeaveRequests.IsLeaveRequestUnderManagerAsync(leaveRequestId, currentUser.EmployeeId.Value);
            if (!underManager && currentUser.EmployeeId != existing.EmployeeId)
                return ApiResponse<LeaveRequestResponseDto>.Forbidden("You are not authorized to approve/reject this request.");

            if (request.IsApproved)
            {
                existing.Status = "APPROVED";
                existing.ApprovedBy = currentUser.EmployeeId;
                existing.ApprovedAt = DateTime.Now;

                var balance = await _leaveBalanceRepo.FirstOrDefaultAsync(b =>
                    b.EmployeeId == existing.EmployeeId &&
                    b.LeaveTypeId == existing.LeaveTypeId &&
                    b.Year == existing.StartDate.Year);

                if (balance != null)
                {
                    balance.UsedDays += existing.TotalDays;
                    balance.UpdatedAt = DateTime.Now;
                    _leaveBalanceRepo.Update(balance);
                }
            }
            else
            {
                existing.Status = "REJECTED";
                existing.RejectionReason = request.RejectionReason;
                existing.ApprovedBy = currentUser.EmployeeId;
                existing.ApprovedAt = DateTime.Now;
            }

            existing.UpdatedAt = DateTime.Now;
            _unitOfWork.LeaveRequests.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<LeaveRequestResponseDto>.Ok(MapLeaveRequestToDto(existing),
                request.IsApproved ? "Leave request approved." : "Leave request rejected.");
        }

        public async Task<ApiResponse<List<LeaveTypeResponseDto>>> GetLeaveTypesAsync(bool? isActive = null)
        {
            List<LeaveType> types;
            if (isActive.HasValue)
                types = await _leaveTypeRepo.FindAsync(lt => lt.IsActive == isActive.Value);
            else
                types = await _leaveTypeRepo.GetAllAsync();

            var dtos = types.Select(MapLeaveTypeToDto).ToList();
            return ApiResponse<List<LeaveTypeResponseDto>>.Ok(dtos);
        }

        public async Task<ApiResponse<LeaveTypeResponseDto>> CreateLeaveTypeAsync(
            CurrentUser currentUser,
            CreateLeaveTypeRequestDto request)
        {
            var existing = await _leaveTypeRepo.FirstOrDefaultAsync(lt => lt.LeaveTypeName == request.LeaveTypeName);
            if (existing != null)
                return ApiResponse<LeaveTypeResponseDto>.Fail("Leave type name already exists.");

            var entity = new LeaveType
            {
                LeaveTypeName = request.LeaveTypeName,
                MaxDaysPerYear = request.MaxDaysPerYear,
                Description = request.Description,
                IsActive = true
            };

            await _leaveTypeRepo.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<LeaveTypeResponseDto>.Ok(MapLeaveTypeToDto(entity), "Leave type created successfully.", 201);
        }

        public async Task<ApiResponse<LeaveTypeResponseDto>> UpdateLeaveTypeAsync(
            CurrentUser currentUser,
            int leaveTypeId,
            UpdateLeaveTypeRequestDto request)
        {
            var entity = await _leaveTypeRepo.GetByIdAsync(leaveTypeId);
            if (entity == null)
                return ApiResponse<LeaveTypeResponseDto>.NotFound("Leave type not found.");

            if (!string.IsNullOrWhiteSpace(request.LeaveTypeName))
                entity.LeaveTypeName = request.LeaveTypeName;
            if (request.MaxDaysPerYear.HasValue)
                entity.MaxDaysPerYear = request.MaxDaysPerYear.Value;
            if (request.Description != null)
                entity.Description = request.Description;
            if (request.IsActive.HasValue)
                entity.IsActive = request.IsActive.Value;

            _leaveTypeRepo.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<LeaveTypeResponseDto>.Ok(MapLeaveTypeToDto(entity), "Leave type updated successfully.");
        }

        public async Task<ApiResponse<bool>> DeactivateLeaveTypeAsync(
            CurrentUser currentUser,
            int leaveTypeId)
        {
            var entity = await _leaveTypeRepo.GetByIdAsync(leaveTypeId);
            if (entity == null)
                return ApiResponse<bool>.NotFound("Leave type not found.");

            entity.IsActive = false;
            _leaveTypeRepo.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Leave type deactivated successfully.");
        }

        public async Task<ApiResponse<bool>> ActivateLeaveTypeAsync(
            CurrentUser currentUser,
            int leaveTypeId)
        {
            var entity = await _leaveTypeRepo.GetByIdAsync(leaveTypeId);
            if (entity == null)
                return ApiResponse<bool>.NotFound("Leave type not found.");

            entity.IsActive = true;
            _leaveTypeRepo.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Leave type activated successfully.");
        }

        public async Task<ApiResponse<LeaveBalanceResponseDto>> SetLeaveBalanceAsync(
            CurrentUser currentUser,
            SetLeaveBalanceRequestDto request)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId);
            if (employee == null)
                return ApiResponse<LeaveBalanceResponseDto>.Fail("Employee not found.");

            var leaveType = await _leaveTypeRepo.GetByIdAsync(request.LeaveTypeId);
            if (leaveType == null)
                return ApiResponse<LeaveBalanceResponseDto>.Fail("Leave type not found.");

            var balance = await _leaveBalanceRepo.FirstOrDefaultAsync(b =>
                b.EmployeeId == request.EmployeeId &&
                b.LeaveTypeId == request.LeaveTypeId &&
                b.Year == request.Year);

            if (balance == null)
            {
                balance = new LeaveBalance
                {
                    EmployeeId = request.EmployeeId,
                    LeaveTypeId = request.LeaveTypeId,
                    Year = request.Year,
                    TotalDays = request.TotalDays,
                    UsedDays = request.UsedDays,
                    UpdatedAt = DateTime.Now
                };
                await _leaveBalanceRepo.AddAsync(balance);
            }
            else
            {
                balance.TotalDays = request.TotalDays;
                balance.UsedDays = request.UsedDays;
                balance.UpdatedAt = DateTime.Now;
                _leaveBalanceRepo.Update(balance);
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = MapLeaveBalanceToDto(balance, employee, leaveType);
            return ApiResponse<LeaveBalanceResponseDto>.Ok(dto, "Leave balance saved successfully.");
        }

        public async Task<ApiResponse<List<LeaveBalanceResponseDto>>> GetLeaveBalancesByEmployeeAsync(
            CurrentUser currentUser,
            int employeeId,
            int? year)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
            if (employee == null)
                return ApiResponse<List<LeaveBalanceResponseDto>>.Fail("Employee not found.");

            var query = _leaveBalanceRepo.Query()
                .Include(b => b.LeaveType)
                .Where(b => b.EmployeeId == employeeId);

            if (year.HasValue)
                query = query.Where(b => b.Year == year.Value);

            var balances = await query.ToListAsync();

            var dtos = balances.Select(b => MapLeaveBalanceToDto(b, employee, b.LeaveType)).ToList();
            return ApiResponse<List<LeaveBalanceResponseDto>>.Ok(dtos);
        }

        private static LeaveRequestResponseDto MapLeaveRequestToDto(LeaveRequest entity)
        {
            return new LeaveRequestResponseDto
            {
                LeaveRequestId = entity.LeaveRequestId,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = entity.Employee?.EmployeeCode,
                EmployeeName = entity.Employee?.FullName,
                LeaveTypeId = entity.LeaveTypeId,
                LeaveTypeName = entity.LeaveType?.LeaveTypeName,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                TotalDays = entity.TotalDays,
                Reason = entity.Reason,
                Status = entity.Status,
                ApprovedBy = entity.ApprovedBy,
                ApprovedByName = entity.ApprovedByNavigation?.FullName,
                ApprovedAt = entity.ApprovedAt,
                RejectionReason = entity.RejectionReason,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        private static LeaveTypeResponseDto MapLeaveTypeToDto(LeaveType entity)
        {
            return new LeaveTypeResponseDto
            {
                LeaveTypeId = entity.LeaveTypeId,
                LeaveTypeName = entity.LeaveTypeName,
                MaxDaysPerYear = entity.MaxDaysPerYear,
                Description = entity.Description,
                IsActive = entity.IsActive
            };
        }

        private static LeaveBalanceResponseDto MapLeaveBalanceToDto(LeaveBalance entity, Employee? employee, LeaveType? leaveType)
        {
            return new LeaveBalanceResponseDto
            {
                LeaveBalanceId = entity.LeaveBalanceId,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = employee?.EmployeeCode,
                EmployeeName = employee?.FullName,
                LeaveTypeId = entity.LeaveTypeId,
                LeaveTypeName = leaveType?.LeaveTypeName,
                Year = entity.Year,
                TotalDays = entity.TotalDays,
                UsedDays = entity.UsedDays,
                RemainingDays = entity.RemainingDays ?? (entity.TotalDays - entity.UsedDays),
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
