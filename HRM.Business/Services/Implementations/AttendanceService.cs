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
        private readonly INotificationService _notificationService;

        public AttendanceService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
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
                    .ThenInclude(e => e.User)
                        .ThenInclude(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role)
                .AsQueryable();

            // UC: Manager/HR xem được bảng chấm công của toàn bộ trừ Admin
            query = query.Where(a => a.Employee.User == null || 
                                    !a.Employee.User.UserRoles.Any(ur => ur.Role.RoleName == "Admin"));

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

        private ApiResponse<bool> ValidateFinalAttendanceTimes(
            Attendance attendance,
            DateTime? requestedCheckIn,
            DateTime? requestedCheckOut)
        {
            var finalCheckIn =
                requestedCheckIn
                ?? attendance.CheckInTime;

            var finalCheckOut =
                requestedCheckOut
                ?? attendance.CheckOutTime;

            if (!requestedCheckIn.HasValue &&
                !requestedCheckOut.HasValue)
            {
                return ApiResponse<bool>.Fail(
                    "At least one requested time is required.");
            }

            if (requestedCheckIn.HasValue &&
                requestedCheckIn.Value.Date != attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue).Date)
            {
                return ApiResponse<bool>.Fail(
                    "Requested check-in time must belong to the attendance date.");
            }

            if (requestedCheckOut.HasValue &&
                requestedCheckOut.Value.Date != attendance.AttendanceDate.ToDateTime(TimeOnly.MinValue).Date)
            {
                return ApiResponse<bool>.Fail(
                    "Requested check-out time must belong to the attendance date.");
            }

            if (finalCheckIn.HasValue &&
                finalCheckOut.HasValue &&
                finalCheckOut.Value <= finalCheckIn.Value)
            {
                return ApiResponse<bool>.Fail(
                    "Requested check-out must be later than check-in.");
            }

            return ApiResponse<bool>.Ok(true);
        }

        private async Task SendApprovalNotificationAsync(AttendanceAdjustment adjustment, string status)
        {
            try
            {
                if (adjustment.RequestedByNavigation?.UserId.HasValue == true)
                {
                    int userId = adjustment.RequestedByNavigation.UserId.Value;
                    string dateStr = adjustment.Attendance?.AttendanceDate.ToString("dd/MM/yyyy") ?? "";
                    string title = $"Attendance adjustment {status.ToLowerInvariant()}";
                    string content = $"Your attendance adjustment request for {dateStr} has been {status.ToLowerInvariant()}.";
                    string type = "SYSTEM";
                    
                    await _notificationService.SendSystemNotificationAsync(title, content, type, new List<int> { userId });
                }
            }
            catch
            {
                // Safe catch for notification issues to prevent transaction rollbacks
            }
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> CreateAdjustmentRequestAsync(
            CurrentUser currentUser,
            CreateAttendanceAdjustmentRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Employee profile not found.");

            var employeeId = currentUser.EmployeeId.Value;

            if (request.AttendanceId <= 0)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Attendance record not found.");

            try
            {
                var attendance = await _unitOfWork.Attendances.GetByIdAsync(request.AttendanceId);
                if (attendance == null)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.NotFound("Attendance record not found.");

                if (attendance.EmployeeId != employeeId)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("You can only adjust your own attendance.");

                var hasPending = await _unitOfWork.AttendanceAdjustments.Query()
                    .AnyAsync(aa => aa.AttendanceId == request.AttendanceId && aa.Status == "PENDING");
                if (hasPending)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("A pending adjustment already exists for this attendance.");

                if (request.Reason == null || string.IsNullOrWhiteSpace(request.Reason.Trim()))
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Reason is required.");

                var validationResult = ValidateFinalAttendanceTimes(attendance, request.RequestedCheckInTime, request.RequestedCheckOutTime);
                if (!validationResult.Success)
                {
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail(validationResult.Message);
                }

                var entity = new AttendanceAdjustment
                {
                    AttendanceId = request.AttendanceId,
                    RequestedBy = employeeId,
                    RequestedCheckInTime = request.RequestedCheckInTime,
                    RequestedCheckOutTime = request.RequestedCheckOutTime,
                    Reason = request.Reason.Trim(),
                    Status = "PENDING",
                    ApprovedBy = null,
                    ApprovedAt = null,
                    RejectionReason = null,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = null
                };

                await _unitOfWork.AttendanceAdjustments.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();

                var created = await _unitOfWork.AttendanceAdjustments.Query()
                    .Include(a => a.RequestedByNavigation)
                    .Include(a => a.ApprovedByNavigation)
                    .Include(a => a.Attendance)
                    .FirstAsync(a => a.AdjustmentId == entity.AdjustmentId);

                return ApiResponse<AttendanceAdjustmentResponseDto>.Ok(MapToAdjustmentDto(created), "Attendance adjustment request created successfully.", 201);
            }
            catch (Exception)
            {
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("An error occurred while processing the attendance adjustment request.");
            }
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> UpdateAdjustmentRequestAsync(
            CurrentUser currentUser,
            int adjustmentId,
            UpdateAttendanceAdjustmentRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Employee profile not found.");

            try
            {
                var existing = await _unitOfWork.AttendanceAdjustments.Query()
                    .Include(a => a.RequestedByNavigation)
                    .Include(a => a.ApprovedByNavigation)
                    .Include(a => a.Attendance)
                    .FirstOrDefaultAsync(a => a.AdjustmentId == adjustmentId);

                if (existing == null)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.NotFound("Attendance adjustment request not found.");

                if (existing.RequestedBy != currentUser.EmployeeId.Value)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("You can only adjust your own attendance.");

                if (existing.Status != "PENDING")
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Only pending requests can be updated.");

                if (request.Reason == null || string.IsNullOrWhiteSpace(request.Reason.Trim()))
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Reason is required.");

                var attendance = existing.Attendance;
                if (attendance == null)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.NotFound("Attendance record not found.");

                var validationResult = ValidateFinalAttendanceTimes(attendance, request.RequestedCheckInTime, request.RequestedCheckOutTime);
                if (!validationResult.Success)
                {
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail(validationResult.Message);
                }

                existing.RequestedCheckInTime = request.RequestedCheckInTime;
                existing.RequestedCheckOutTime = request.RequestedCheckOutTime;
                existing.Reason = request.Reason.Trim();
                existing.UpdatedAt = DateTime.Now;

                _unitOfWork.AttendanceAdjustments.Update(existing);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<AttendanceAdjustmentResponseDto>.Ok(MapToAdjustmentDto(existing), "Attendance adjustment request updated successfully.");
            }
            catch (Exception)
            {
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("An error occurred while processing the attendance adjustment request.");
            }
        }

        public async Task<ApiResponse<bool>> CancelAdjustmentRequestAsync(
            CurrentUser currentUser,
            int adjustmentId)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<bool>.Fail("Employee profile not found.");

            try
            {
                var existing = await _unitOfWork.AttendanceAdjustments.Query()
                    .FirstOrDefaultAsync(a => a.AdjustmentId == adjustmentId);

                if (existing == null)
                    return ApiResponse<bool>.NotFound("Attendance adjustment request not found.");

                if (existing.RequestedBy != currentUser.EmployeeId.Value)
                    return ApiResponse<bool>.Forbidden("You can only adjust your own attendance.");

                if (existing.Status != "PENDING")
                    return ApiResponse<bool>.Fail("Only pending requests can be cancelled.");

                existing.Status = "CANCELLED";
                existing.UpdatedAt = DateTime.Now;

                _unitOfWork.AttendanceAdjustments.Update(existing);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<bool>.Ok(true, "Attendance adjustment request cancelled successfully.");
            }
            catch (Exception)
            {
                return ApiResponse<bool>.Fail("An error occurred while processing the attendance adjustment request.");
            }
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> ApproveOrRejectAdjustmentAsync(
            CurrentUser currentUser,
            int adjustmentId,
            ApproveAttendanceAdjustmentRequestDto request)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Employee profile not found.");

            var managerEmployeeId = currentUser.EmployeeId.Value;

            try
            {
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
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Only pending requests can be approved or rejected.");

                // Self-approval check
                if (managerEmployeeId == existing.RequestedBy)
                {
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("You cannot approve or reject your own request.");
                }

                var isUnderManager = existing.RequestedByNavigation.EmployeeAssignments
                    .Any(ea => ea.EndDate == null && ea.Department.ManagerEmployeeId == managerEmployeeId);

                if (!isUnderManager && !currentUser.IsAdmin())
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("The employee is outside your managed department.");

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    if (request.IsApproved)
                    {
                        var attendance = existing.Attendance;
                        if (attendance == null)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return ApiResponse<AttendanceAdjustmentResponseDto>.NotFound("Attendance record not found.");
                        }

                        var finalCheckIn = existing.RequestedCheckInTime ?? attendance.CheckInTime;
                        var finalCheckOut = existing.RequestedCheckOutTime ?? attendance.CheckOutTime;

                        var timeValidation = ValidateFinalAttendanceTimes(attendance, existing.RequestedCheckInTime, existing.RequestedCheckOutTime);
                        if (!timeValidation.Success)
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return ApiResponse<AttendanceAdjustmentResponseDto>.Fail(timeValidation.Message);
                        }

                        if (finalCheckIn.HasValue && finalCheckOut.HasValue)
                        {
                            var diffHours = (decimal)(finalCheckOut.Value - finalCheckIn.Value).TotalHours;

                            if (diffHours <= 0)
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Working hours must be greater than 0.");
                            }

                            if (diffHours > 24)
                            {
                                await _unitOfWork.RollbackTransactionAsync();
                                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Working hours cannot exceed 24 hours.");
                            }

                            attendance.WorkingHours = Math.Round(diffHours, 2);
                        }

                        if (existing.RequestedCheckInTime.HasValue)
                        {
                            attendance.CheckInTime = existing.RequestedCheckInTime;
                        }
                        if (existing.RequestedCheckOutTime.HasValue)
                        {
                            attendance.CheckOutTime = existing.RequestedCheckOutTime;
                        }
                        attendance.UpdatedAt = DateTime.Now;
                        _unitOfWork.Attendances.Update(attendance);

                        existing.Status = "APPROVED";
                        existing.RejectionReason = null;
                    }
                    else
                    {
                        if (request.RejectionReason == null || string.IsNullOrWhiteSpace(request.RejectionReason.Trim()))
                        {
                            await _unitOfWork.RollbackTransactionAsync();
                            return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Rejection reason is required.");
                        }
                        existing.Status = "REJECTED";
                        existing.RejectionReason = request.RejectionReason.Trim();
                    }

                    existing.ApprovedBy = managerEmployeeId;
                    existing.ApprovedAt = DateTime.Now;
                    existing.UpdatedAt = DateTime.Now;

                    _unitOfWork.AttendanceAdjustments.Update(existing);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitTransactionAsync();
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("An error occurred while processing the attendance adjustment request.");
                }

                // Send notification after successful commit
                if (existing.Status == "APPROVED" || existing.Status == "REJECTED")
                {
                    await SendApprovalNotificationAsync(existing, existing.Status);
                }

                return ApiResponse<AttendanceAdjustmentResponseDto>.Ok(MapToAdjustmentDto(existing), request.IsApproved ? "Request approved successfully." : "Request rejected successfully.");
            }
            catch (Exception)
            {
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("An error occurred while processing the attendance adjustment request.");
            }
        }

        public async Task<ApiResponse<List<AttendanceAdjustmentResponseDto>>> GetPendingAdjustmentRequestsAsync(
            CurrentUser currentUser,
            int? employeeId = null,
            int? month = null,
            int? year = null)
        {
            if (month.HasValue && (month.Value < 1 || month.Value > 12))
            {
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Month must be between 1 and 12.");
            }

            if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
            {
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Year is invalid.");
            }

            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Employee profile not found.");

            try
            {
                var managerEmployeeId = currentUser.EmployeeId.Value;

                var query = _unitOfWork.AttendanceAdjustments.Query()
                    .Include(a => a.RequestedByNavigation)
                        .ThenInclude(e => e.EmployeeAssignments)
                            .ThenInclude(ea => ea.Department)
                    .Include(a => a.ApprovedByNavigation)
                    .Include(a => a.Attendance)
                    .Where(a => a.Status == "PENDING");

                // Filter by manager's department members (unless Admin)
                if (!currentUser.IsAdmin())
                {
                    query = query.Where(a => a.RequestedByNavigation.EmployeeAssignments.Any(ea => ea.EndDate == null && ea.Department.ManagerEmployeeId == managerEmployeeId));
                }

                if (employeeId.HasValue)
                {
                    query = query.Where(a => a.RequestedBy == employeeId.Value);
                }

                if (month.HasValue)
                {
                    query = query.Where(a => a.Attendance.AttendanceDate.Month == month.Value);
                }

                if (year.HasValue)
                {
                    query = query.Where(a => a.Attendance.AttendanceDate.Year == year.Value);
                }

                var pending = await query
                    .OrderBy(a => a.CreatedAt)
                    .ToListAsync();

                var dtos = pending.Select(MapToAdjustmentDto).ToList();
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Ok(dtos, "Pending adjustment requests retrieved successfully.");
            }
            catch (Exception)
            {
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("An error occurred while processing the attendance adjustment request.");
            }
        }

        public async Task<ApiResponse<List<AttendanceAdjustmentResponseDto>>> GetMyAdjustmentRequestsAsync(
            CurrentUser currentUser,
            string? status = null,
            int? month = null,
            int? year = null)
        {
            if (month.HasValue && (month.Value < 1 || month.Value > 12))
            {
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Month must be between 1 and 12.");
            }

            if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
            {
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Year is invalid.");
            }

            if (!string.IsNullOrEmpty(status))
            {
                status = status.Trim().ToUpperInvariant();
                var validStatuses = new[] { "PENDING", "APPROVED", "REJECTED", "CANCELLED" };
                if (!validStatuses.Contains(status))
                {
                    return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Invalid status filter.");
                }
            }

            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("Employee profile not found.");

            try
            {
                var employeeId = currentUser.EmployeeId.Value;

                var query = _unitOfWork.AttendanceAdjustments.Query()
                    .Include(a => a.RequestedByNavigation)
                        .ThenInclude(e => e.EmployeeAssignments)
                            .ThenInclude(ea => ea.Department)
                    .Include(a => a.ApprovedByNavigation)
                    .Include(a => a.Attendance)
                    .Where(a => a.RequestedBy == employeeId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.Status == status);
                }

                if (month.HasValue)
                {
                    query = query.Where(a => a.Attendance.AttendanceDate.Month == month.Value);
                }

                if (year.HasValue)
                {
                    query = query.Where(a => a.Attendance.AttendanceDate.Year == year.Value);
                }

                var adjustments = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var dtos = adjustments.Select(MapToAdjustmentDto).ToList();
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Ok(dtos, "My adjustment requests retrieved successfully.");
            }
            catch (Exception)
            {
                return ApiResponse<List<AttendanceAdjustmentResponseDto>>.Fail("An error occurred while processing the attendance adjustment request.");
            }
        }

        public async Task<ApiResponse<List<AdjustableAttendanceDto>>> GetAdjustableAttendancesAsync(
            CurrentUser currentUser,
            int? month = null,
            int? year = null)
        {
            if (month.HasValue && (month.Value < 1 || month.Value > 12))
            {
                return ApiResponse<List<AdjustableAttendanceDto>>.Fail("Month must be between 1 and 12.");
            }

            if (year.HasValue && (year.Value < 2000 || year.Value > 2100))
            {
                return ApiResponse<List<AdjustableAttendanceDto>>.Fail("Year is invalid.");
            }

            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<List<AdjustableAttendanceDto>>.Fail("Employee profile not found.");

            try
            {
                var employeeId = currentUser.EmployeeId.Value;

                var query = _unitOfWork.Attendances.Query()
                    .Include(a => a.AttendanceAdjustments)
                    .Where(a => a.EmployeeId == employeeId);

                if (month.HasValue)
                {
                    query = query.Where(a => a.AttendanceDate.Month == month.Value);
                }

                if (year.HasValue)
                {
                    query = query.Where(a => a.AttendanceDate.Year == year.Value);
                }

                // Exclude attendances that already have a PENDING adjustment
                query = query.Where(a => !a.AttendanceAdjustments.Any(aa => aa.Status == "PENDING"));

                var attendances = await query
                    .OrderByDescending(a => a.AttendanceDate)
                    .ToListAsync();

                var dtos = attendances.Select(a => new AdjustableAttendanceDto
                {
                    AttendanceId = a.AttendanceId,
                    AttendanceDate = a.AttendanceDate,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    WorkingHours = a.WorkingHours,
                    Status = a.Status,
                    HasPendingAdjustment = false
                }).ToList();

                return ApiResponse<List<AdjustableAttendanceDto>>.Ok(dtos, "Adjustable attendances retrieved successfully.");
            }
            catch (Exception)
            {
                return ApiResponse<List<AdjustableAttendanceDto>>.Fail("An error occurred while processing the attendance adjustment request.");
            }
        }

        public async Task<ApiResponse<AttendanceAdjustmentResponseDto>> GetAdjustmentDetailAsync(
            CurrentUser currentUser,
            int adjustmentId)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("Employee profile not found.");

            try
            {
                var employeeId = currentUser.EmployeeId.Value;

                var existing = await _unitOfWork.AttendanceAdjustments.Query()
                    .Include(a => a.RequestedByNavigation)
                        .ThenInclude(e => e.EmployeeAssignments)
                            .ThenInclude(ea => ea.Department)
                    .Include(a => a.ApprovedByNavigation)
                    .Include(a => a.Attendance)
                    .FirstOrDefaultAsync(a => a.AdjustmentId == adjustmentId);

                if (existing == null)
                    return ApiResponse<AttendanceAdjustmentResponseDto>.NotFound("Attendance adjustment request not found.");

                // Role boundary validation
                if (currentUser.IsEmployee() && existing.RequestedBy != employeeId)
                {
                    if (!currentUser.IsManager() && !currentUser.IsAdmin() && !currentUser.IsHr())
                    {
                        return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("You can only view your own adjustment requests.");
                    }
                }

                if (currentUser.IsManager() && !currentUser.IsAdmin() && !currentUser.IsHr())
                {
                    bool isOwnRequest = existing.RequestedBy == employeeId;
                    bool isUnderManager = existing.RequestedByNavigation.EmployeeAssignments
                        .Any(ea => ea.EndDate == null && ea.Department.ManagerEmployeeId == employeeId);

                    if (!isOwnRequest && !isUnderManager)
                    {
                        return ApiResponse<AttendanceAdjustmentResponseDto>.Forbidden("The employee is outside your managed department.");
                    }
                }

                return ApiResponse<AttendanceAdjustmentResponseDto>.Ok(MapToAdjustmentDto(existing), "Attendance adjustment request details loaded.");
            }
            catch (Exception)
            {
                return ApiResponse<AttendanceAdjustmentResponseDto>.Fail("An error occurred while processing the attendance adjustment request.");
            }
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
            var dto = new AttendanceAdjustmentResponseDto
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

            if (entity.Attendance != null)
            {
                dto.AttendanceDate = entity.Attendance.AttendanceDate;
                dto.CurrentCheckInTime = entity.Attendance.CheckInTime;
                dto.CurrentCheckOutTime = entity.Attendance.CheckOutTime;
            }

            if (entity.RequestedByNavigation != null)
            {
                dto.EmployeeCode = entity.RequestedByNavigation.EmployeeCode;
                if (entity.RequestedByNavigation.EmployeeAssignments != null)
                {
                    var activeAssignment = entity.RequestedByNavigation.EmployeeAssignments
                        .FirstOrDefault(ea => ea.EndDate == null);
                    if (activeAssignment?.Department != null)
                    {
                        dto.DepartmentName = activeAssignment.Department.DepartmentName;
                    }
                }
            }

            return dto;
        }
    }
}