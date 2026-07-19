using HRM.Business.Common;
using HRM.Business.DTOs.Attendances;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AttendanceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<CheckInResponseDto>> CheckInAsync(CurrentUser currentUser)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<CheckInResponseDto>.Fail("Employee profile not found.");

            var today = DateOnly.FromDateTime(DateTime.Now);
            var employeeId = currentUser.EmployeeId.Value;

            var existingAttendance = await _unitOfWork.Attendances.Query()
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == today);

            if (existingAttendance != null)
                return ApiResponse<CheckInResponseDto>.Fail("You have already checked in today.");

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                AttendanceDate = today,
                CheckInTime = DateTime.Now,
                Status = "PRESENT", 
                WorkingHours = 0,
                CreatedAt = DateTime.Now
            };

            if (attendance.CheckInTime.Value.TimeOfDay > new TimeSpan(8, 30, 0))
            {
                attendance.Status = "LATE";
            }

            await _unitOfWork.Attendances.AddAsync(attendance);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<CheckInResponseDto>.Ok(new CheckInResponseDto
            {
                AttendanceId = attendance.AttendanceId,
                EmployeeId = attendance.EmployeeId,
                AttendanceDate = attendance.AttendanceDate,
                CheckInTime = attendance.CheckInTime,
                Status = attendance.Status,
                Message = "Checked in successfully."
            });
        }

        public async Task<ApiResponse<CheckOutResponseDto>> CheckOutAsync(CurrentUser currentUser)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<CheckOutResponseDto>.Fail("Employee profile not found.");

            var today = DateOnly.FromDateTime(DateTime.Now);
            var employeeId = currentUser.EmployeeId.Value;

            var attendance = await _unitOfWork.Attendances.Query()
                .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == today);

            if (attendance == null)
                return ApiResponse<CheckOutResponseDto>.Fail("No check-in record found for today. Please check in first.");

            if (attendance.CheckOutTime.HasValue)
                return ApiResponse<CheckOutResponseDto>.Fail("You have already checked out today.");

            attendance.CheckOutTime = DateTime.Now;
            attendance.WorkingHours = (decimal)(attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours;
            attendance.WorkingHours = Math.Round(attendance.WorkingHours, 2);
            attendance.UpdatedAt = DateTime.Now;

            _unitOfWork.Attendances.Update(attendance);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<CheckOutResponseDto>.Ok(new CheckOutResponseDto
            {
                AttendanceId = attendance.AttendanceId,
                EmployeeId = attendance.EmployeeId,
                AttendanceDate = attendance.AttendanceDate,
                CheckInTime = attendance.CheckInTime,
                CheckOutTime = attendance.CheckOutTime,
                WorkingHours = attendance.WorkingHours,
                Status = attendance.Status,
                Message = "Checked out successfully."
            });
        }

        public async Task<ApiResponse<PagedResult<AttendanceResponseDto>>> GetMyAttendanceHistoryAsync(
            CurrentUser currentUser,
            AttendanceFilterDto filter)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<PagedResult<AttendanceResponseDto>>.Fail("Employee profile not found.");

            var query = _unitOfWork.Attendances.Query()
                .Where(a => a.EmployeeId == currentUser.EmployeeId.Value)
                .AsQueryable();

            if (filter.FromDate.HasValue)
                query = query.Where(a => a.AttendanceDate >= filter.FromDate.Value);
            
            if (filter.ToDate.HasValue)
                query = query.Where(a => a.AttendanceDate <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(a => a.Status == filter.Status);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.AttendanceDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = items.Select(MapToDto).ToList();
            var pagedResult = PagedResult<AttendanceResponseDto>.Create(result, filter.PageNumber, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<AttendanceResponseDto>>.Ok(pagedResult);
        }

        public async Task<ApiResponse<PagedResult<AttendanceResponseDto>>> GetAttendanceSheetAsync(
            CurrentUser currentUser,
            AttendanceFilterDto filter)
        {
            var query = _unitOfWork.Attendances.Query()
                .Include(a => a.Employee)
                .AsQueryable();

            if (filter.EmployeeId.HasValue)
                query = query.Where(a => a.EmployeeId == filter.EmployeeId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(a => a.AttendanceDate >= filter.FromDate.Value);
            
            if (filter.ToDate.HasValue)
                query = query.Where(a => a.AttendanceDate <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(a => a.Status == filter.Status);

            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                var kw = filter.Keyword.ToLower();
                query = query.Where(a => a.Employee.FullName.ToLower().Contains(kw) || a.Employee.EmployeeCode.ToLower().Contains(kw));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.AttendanceDate)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = items.Select(MapToDto).ToList();
            var pagedResult = PagedResult<AttendanceResponseDto>.Create(result, filter.PageNumber, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<AttendanceResponseDto>>.Ok(pagedResult);
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> CreateAdjustmentRequestAsync(
            CurrentUser currentUser,
            CreateAttendanceAdjustmentRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Employee profile not found.");

            var attendance = await _unitOfWork.Attendances.GetByIdAsync(request.AttendanceId);
            if (attendance == null)
                return ApiResponse<AttendanceAdjustmentResponseDto>.NotFound("Attendance record not found.");

            if (attendance.EmployeeId != currentUser.EmployeeId.Value)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("You can only request adjustment for your own attendance.");

            var hasPending = await _unitOfWork.AttendanceAdjustments.Query()
                .AnyAsync(aa => aa.AttendanceId == request.AttendanceId && aa.Status == "PENDING");
            if (hasPending)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("There is already a pending adjustment request for this attendance record.");

            if (request.RequestedCheckInTime.HasValue && request.RequestedCheckOutTime.HasValue)
            {
                if (request.RequestedCheckOutTime.Value < request.RequestedCheckInTime.Value)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Requested check-out time must be greater than or equal to check-in time.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Reason is required.");

            var entity = new AttendanceAdjustment
            {
                AttendanceId = request.AttendanceId,
                RequestedBy = currentUser.EmployeeId.Value,
                RequestedCheckInTime = request.RequestedCheckInTime,
                RequestedCheckOutTime = request.RequestedCheckOutTime,
                Reason = request.Reason,
                Status = "PENDING",
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.AttendanceAdjustments.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.AttendanceAdjustments.Query()
                .Include(a => a.RequestedByNavigation)
                .Include(a => a.ApprovedByNavigation)
                .FirstAsync(a => a.AdjustmentId == entity.AdjustmentId);

            return ApiResponse<AttendanceAdjustmentResponseDto>.Ok(MapToAdjustmentDto(created), "Attendance adjustment request created successfully.", 201);
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> UpdateAdjustmentRequestAsync(
            CurrentUser currentUser,
            int adjustmentId,
            UpdateAttendanceAdjustmentRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Employee profile not found.");

            var existing = await _unitOfWork.AttendanceAdjustments.Query()
                .Include(a => a.RequestedByNavigation)
                .Include(a => a.ApprovedByNavigation)
                .FirstOrDefaultAsync(a => a.AdjustmentId == adjustmentId);

            if (existing == null)
                return ApiResponse<AttendanceAdjustmentResponseDto>.NotFound("Attendance adjustment request not found.");

            if (existing.RequestedBy != currentUser.EmployeeId.Value)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("You can only update your own adjustment requests.");

            if (existing.Status != "PENDING")
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Only PENDING requests can be updated.");

            if (request.RequestedCheckInTime.HasValue && request.RequestedCheckOutTime.HasValue)
            {
                if (request.RequestedCheckOutTime.Value < request.RequestedCheckInTime.Value)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Requested check-out time must be greater than or equal to check-in time.");
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Reason is required.");

            existing.RequestedCheckInTime = request.RequestedCheckInTime;
            existing.RequestedCheckOutTime = request.RequestedCheckOutTime;
            existing.Reason = request.Reason;
            existing.UpdatedAt = DateTime.Now;

            _unitOfWork.AttendanceAdjustments.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<AttendanceAdjustmentResponseDto>.Ok(MapToAdjustmentDto(existing), "Attendance adjustment request updated successfully.");
        }

        public async Task<ApiResponse<bool>> CancelAdjustmentRequestAsync(
            CurrentUser currentUser,
            int adjustmentId)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<bool>.Fail("Employee profile not found.");

            var existing = await _unitOfWork.AttendanceAdjustments.Query()
                .FirstOrDefaultAsync(a => a.AdjustmentId == adjustmentId);

            if (existing == null)
                return ApiResponse<bool>.NotFound("Attendance adjustment request not found.");

            if (existing.RequestedBy != currentUser.EmployeeId.Value)
                return ApiResponse<bool>.Forbidden("You can only cancel your own adjustment requests.");

            if (existing.Status != "PENDING")
                return ApiResponse<bool>.Fail("Only PENDING requests can be cancelled.");

            existing.Status = "CANCELLED";
            existing.UpdatedAt = DateTime.Now;

            _unitOfWork.AttendanceAdjustments.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Attendance adjustment request cancelled successfully.");
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> ApproveOrRejectAdjustmentAsync(
            CurrentUser currentUser,
            int adjustmentId,
            ApproveAttendanceAdjustmentRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Employee profile not found.");

            var managerEmployeeId = currentUser.EmployeeId.Value;

            var existing = await _unitOfWork.AttendanceAdjustments.Query()
                .Include(a => a.RequestedByNavigation)
                    .ThenInclude(e => e.EmployeeAssignments)
                        .ThenInclude(ea => ea.Department)
                .Include(a => a.ApprovedByNavigation)
                .Include(a => a.Attendance)
                .FirstOrDefaultAsync(a => a.AdjustmentId == adjustmentId);

            if (existing == null)
                return ApiResponse<AttendanceAdjustmentResponseDto>.NotFound("Attendance adjustment request not found.");

            if (existing.Status != "PENDING")
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Only PENDING requests can be approved or rejected.");

            var isUnderManager = existing.RequestedByNavigation.EmployeeAssignments
                .Any(ea => ea.EndDate == null && ea.Department.ManagerEmployeeId == managerEmployeeId);

            if (!isUnderManager)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("You can only approve/reject requests from employees in your managed department.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (request.IsApproved)
                {
                    var attendance = existing.Attendance;
                    if (existing.RequestedCheckInTime.HasValue)
                    {
                        attendance.CheckInTime = existing.RequestedCheckInTime;
                    }
                    if (existing.RequestedCheckOutTime.HasValue)
                    {
                        attendance.CheckOutTime = existing.RequestedCheckOutTime;
                    }
                    if (attendance.CheckInTime.HasValue && attendance.CheckOutTime.HasValue)
                    {
                        attendance.WorkingHours = Math.Round((decimal)(attendance.CheckOutTime.Value - attendance.CheckInTime.Value).TotalHours, 2);
                        if (attendance.WorkingHours < 0) attendance.WorkingHours = 0;
                    }
                    attendance.UpdatedAt = DateTime.Now;
                    _unitOfWork.Attendances.Update(attendance);

                    existing.Status = "APPROVED";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(request.RejectionReason))
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Rejection reason is required when rejecting a request.");
                    }
                    existing.Status = "REJECTED";
                    existing.RejectionReason = request.RejectionReason;
                }

                existing.ApprovedBy = managerEmployeeId;
                existing.ApprovedAt = DateTime.Now;
                existing.UpdatedAt = DateTime.Now;

                _unitOfWork.AttendanceAdjustments.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("An error occurred during approval process: " + ex.Message);
            }

            return ApiResponse<AttendanceAdjustmentResponseDto>.Ok(MapToAdjustmentDto(existing), request.IsApproved ? "Request approved successfully." : "Request rejected successfully.");
        }

        public async Task<ApiResponse<List<AttendanceAdjustmentResponseDto>>> GetPendingAdjustmentRequestsAsync(
            CurrentUser currentUser)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Employee profile not found.");

            var managerEmployeeId = currentUser.EmployeeId.Value;

            var pending = await _unitOfWork.AttendanceAdjustments.Query()
                .Include(a => a.RequestedByNavigation)
                    .ThenInclude(e => e.EmployeeAssignments)
                        .ThenInclude(ea => ea.Department)
                .Include(a => a.ApprovedByNavigation)
                .Include(a => a.Attendance)
                .Where(a => a.Status == "PENDING" &&
                            a.RequestedByNavigation.EmployeeAssignments.Any(ea => ea.EndDate == null && ea.Department.ManagerEmployeeId == managerEmployeeId))
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            var dtos = pending.Select(MapToAdjustmentDto).ToList();
            return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Ok(dtos, "Pending adjustment requests retrieved successfully.");
        }

        public async Task<ApiResponse<List<AttendanceAdjustmentResponseDto>>> GetMyAdjustmentRequestsAsync(
            CurrentUser currentUser)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Employee profile not found.");

            var employeeId = currentUser.EmployeeId.Value;

            var adjustments = await _unitOfWork.AttendanceAdjustments.Query()
                .Include(a => a.RequestedByNavigation)
                .Include(a => a.ApprovedByNavigation)
                .Where(a => a.RequestedBy == employeeId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            var dtos = adjustments.Select(MapToAdjustmentDto).ToList();
            return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Ok(dtos, "My adjustment requests retrieved successfully.");
        }

        private AttendanceResponseDto MapToDto(Attendance a)
        {
            return new AttendanceResponseDto
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee?.FullName,
                EmployeeCode = a.Employee?.EmployeeCode,
                AttendanceDate = a.AttendanceDate,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                WorkingHours = a.WorkingHours,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            };
        }

        private AttendanceAdjustmentResponseDto MapToAdjustmentDto(AttendanceAdjustment entity)
        {
            return new AttendanceAdjustmentResponseDto
            {
                AdjustmentId = entity.AdjustmentId,
                AttendanceId = entity.AttendanceId,
                RequestedBy = entity.RequestedBy,
                RequestedByName = entity.RequestedByNavigation?.FullName,
                RequestedCheckInTime = entity.RequestedCheckInTime,
                RequestedCheckOutTime = entity.RequestedCheckOutTime,
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
    }
}