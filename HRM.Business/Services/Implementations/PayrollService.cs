using HRM.Business.Common;
using HRM.Business.DTOs.Payrolls;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class PayrollService : IPayrollService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public PayrollService(IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<List<PayrollResponseDto>>> GenerateMonthlyPayrollAsync(
            CurrentUser currentUser,
            GeneratePayrollRequestDto request)
        {
            if (request.PayrollMonth < 1 || request.PayrollMonth > 12)
                return ApiResponse<List<PayrollResponseDto>>.Fail("Payroll month must be from 1 to 12.");

            if (request.PayrollYear < 2000 || request.PayrollYear > 2100)
                return ApiResponse<List<PayrollResponseDto>>.Fail("Payroll year must be valid.");

            // Get active employees who have an active contract
            var employeeQuery = _unitOfWork.Employees.Query()
                .Include(e => e.Contract)
                .Where(e => e.EmploymentStatus == "ACTIVE" && e.Contract != null && e.Contract.Status == "ACTIVE");

            if (request.EmployeeId.HasValue)
            {
                employeeQuery = employeeQuery.Where(e => e.EmployeeId == request.EmployeeId.Value);
            }

            if (request.DepartmentId.HasValue)
            {
                employeeQuery = employeeQuery.Where(e => e.EmployeeAssignments.Any(ea => ea.DepartmentId == request.DepartmentId.Value && ea.EndDate == null));
            }

            var activeEmployees = await employeeQuery.ToListAsync();

            // Find existing payrolls for the period to prevent duplication
            var existingEmployeeIds = await _unitOfWork.Payrolls.Query()
                .Where(p => p.PayrollMonth == request.PayrollMonth && p.PayrollYear == request.PayrollYear)
                .Select(p => p.EmployeeId)
                .ToListAsync();

            var newPayrolls = new List<Payroll>();
            foreach (var emp in activeEmployees)
            {
                if (existingEmployeeIds.Contains(emp.EmployeeId))
                    continue;

                var payroll = new Payroll
                {
                    EmployeeId = emp.EmployeeId,
                    PayrollMonth = request.PayrollMonth,
                    PayrollYear = request.PayrollYear,
                    BaseSalary = emp.Contract?.Salary ?? 0,
                    TotalAllowance = 0,
                    TotalBonus = 0,
                    TotalOvertime = 0,
                    TotalDeduction = 0,
                    GrossSalary = emp.Contract?.Salary ?? 0,
                    NetSalary = emp.Contract?.Salary ?? 0,
                    Status = "DRAFT",
                    GeneratedByUserId = currentUser.UserId,
                    CreatedAt = DateTime.Now
                };
                newPayrolls.Add(payroll);
            }

            if (newPayrolls.Any())
            {
                await _unitOfWork.Payrolls.AddRangeAsync(newPayrolls);
                await _unitOfWork.SaveChangesAsync();
            }

            // Retrieve all payrolls for the period to return
            var query = _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                .Include(p => p.GeneratedByUser)
                .Include(p => p.ConfirmedByUser)
                .Where(p => p.PayrollMonth == request.PayrollMonth && p.PayrollYear == request.PayrollYear);

            if (request.EmployeeId.HasValue)
            {
                query = query.Where(p => p.EmployeeId == request.EmployeeId.Value);
            }

            if (request.DepartmentId.HasValue)
            {
                query = query.Where(p => p.Employee.EmployeeAssignments.Any(ea => ea.DepartmentId == request.DepartmentId.Value && ea.EndDate == null));
            }

            var allPayrolls = await query.ToListAsync();
            var dtos = allPayrolls.Select(MapToPayrollDto).ToList();

            return ApiResponse<List<PayrollResponseDto>>.Ok(dtos, $"Generated payrolls successfully for period {request.PayrollMonth}/{request.PayrollYear}.");
        }

        public async Task<ApiResponse<List<PayrollResponseDto>>> CalculatePayrollAsync(
            CurrentUser currentUser,
            CalculatePayrollDto request)
        {
            if (request.DefaultAllowance < 0 || request.DefaultBonus < 0 || request.DefaultDeduction < 0 || request.OvertimeRate < 0)
                return ApiResponse<List<PayrollResponseDto>>.Fail("Allowance, bonus, deduction, and overtime rate must be non-negative.");

            var payrolls = await _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                    .ThenInclude(e => e.Contract)
                .Include(p => p.PayrollDetails)
                .Where(p => p.PayrollMonth == request.PayrollMonth &&
                            p.PayrollYear == request.PayrollYear &&
                            p.Status == "DRAFT")
                .ToListAsync();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var payroll in payrolls)
                {
                    // 1. Get base salary from active contract (or fallback to existing base salary)
                    var emp = payroll.Employee;
                    if (emp?.Contract != null && emp.Contract.Status == "ACTIVE")
                    {
                        payroll.BaseSalary = emp.Contract.Salary;
                    }

                    // 2. Clear old automatic details
                    if (payroll.PayrollDetails != null && payroll.PayrollDetails.Any())
                    {
                        var detailsToDelete = payroll.PayrollDetails
                            .Where(d => d.ItemType == "ALLOWANCE" || d.ItemType == "BONUS" || d.ItemType == "DEDUCTION" || d.ItemType == "OVERTIME")
                            .ToList();
                        
                        if (detailsToDelete.Any())
                        {
                            _unitOfWork.PayrollDetails.DeleteRange(detailsToDelete);
                        }
                    }

                    // 3. Find approved, non-transferred overtime requests for the period
                    var otRequests = await _unitOfWork.OvertimeRequests.Query()
                        .Where(ot => ot.EmployeeId == payroll.EmployeeId &&
                                     ot.Status == "APPROVED" &&
                                     !ot.IsTransferredToPayroll &&
                                     ot.Otdate.Month == request.PayrollMonth &&
                                     ot.Otdate.Year == request.PayrollYear)
                        .ToListAsync();

                    var newDetails = new List<PayrollDetail>();
                    decimal totalOvertime = 0;

                    foreach (var ot in otRequests)
                    {
                        var otAmount = Math.Round(ot.TotalHours * request.OvertimeRate, 2);
                        if (otAmount < 0) otAmount = 0;

                        newDetails.Add(new PayrollDetail
                        {
                            PayrollId = payroll.PayrollId,
                            ItemType = "OVERTIME",
                            Amount = otAmount,
                            Description = $"Approved Overtime on {ot.Otdate:dd/MM/yyyy} ({ot.TotalHours} hrs @ {request.OvertimeRate}/hr)",
                            SourceType = "OVERTIME",
                            SourceId = ot.Otid,
                            CreatedAt = DateTime.Now
                        });

                        totalOvertime += otAmount;
                        ot.IsTransferredToPayroll = true;
                        _unitOfWork.OvertimeRequests.Update(ot);
                    }

                    // 4. Create default details
                    if (request.DefaultAllowance > 0)
                    {
                        newDetails.Add(new PayrollDetail
                        {
                            PayrollId = payroll.PayrollId,
                            ItemType = "ALLOWANCE",
                            Amount = request.DefaultAllowance,
                            Description = "Default Monthly Allowance",
                            CreatedAt = DateTime.Now
                        });
                    }

                    if (request.DefaultBonus > 0)
                    {
                        newDetails.Add(new PayrollDetail
                        {
                            PayrollId = payroll.PayrollId,
                            ItemType = "BONUS",
                            Amount = request.DefaultBonus,
                            Description = "Default Monthly Bonus",
                            CreatedAt = DateTime.Now
                        });
                    }

                    if (request.DefaultDeduction > 0)
                    {
                        newDetails.Add(new PayrollDetail
                        {
                            PayrollId = payroll.PayrollId,
                            ItemType = "DEDUCTION",
                            Amount = request.DefaultDeduction,
                            Description = "Default Monthly Deduction",
                            CreatedAt = DateTime.Now
                        });
                    }

                    if (newDetails.Any())
                    {
                        await _unitOfWork.PayrollDetails.AddRangeAsync(newDetails);
                    }

                    // 5. Update payroll totals
                    payroll.TotalAllowance = request.DefaultAllowance > 0 ? request.DefaultAllowance : 0;
                    payroll.TotalBonus = request.DefaultBonus > 0 ? request.DefaultBonus : 0;
                    payroll.TotalDeduction = request.DefaultDeduction > 0 ? request.DefaultDeduction : 0;
                    payroll.TotalOvertime = totalOvertime;

                    payroll.GrossSalary = payroll.BaseSalary + payroll.TotalAllowance + payroll.TotalBonus + payroll.TotalOvertime;
                    payroll.NetSalary = payroll.GrossSalary - payroll.TotalDeduction;
                    if (payroll.NetSalary < 0) payroll.NetSalary = 0;

                    _unitOfWork.Payrolls.Update(payroll);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<List<PayrollResponseDto>>.Fail("Error calculating payroll: " + ex.Message);
            }

            // Reload calculated payrolls to return
            var updatedPayrolls = await _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                .Include(p => p.PayrollDetails)
                .Include(p => p.GeneratedByUser)
                .Include(p => p.ConfirmedByUser)
                .Where(p => p.PayrollMonth == request.PayrollMonth &&
                            p.PayrollYear == request.PayrollYear &&
                            p.Status == "DRAFT")
                .ToListAsync();

            var dtos = updatedPayrolls.Select(MapToPayrollDto).ToList();
            return ApiResponse<List<PayrollResponseDto>>.Ok(dtos, "Payroll calculated successfully.");
        }

        public async Task<ApiResponse<PayrollResponseDto>> GetPayrollDetailAsync(
            CurrentUser currentUser,
            int payrollId)
        {
            var payroll = await _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Position)
                .Include(p => p.PayrollDetails)
                .Include(p => p.GeneratedByUser)
                .Include(p => p.ConfirmedByUser)
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId);

            if (payroll == null)
                return ApiResponse<PayrollResponseDto>.NotFound("Payroll not found.");

            // Role-based auth
            if (currentUser.IsEmployee() && !currentUser.IsAdmin() && !currentUser.IsPayroll() && !currentUser.IsHr())
            {
                if (payroll.EmployeeId != currentUser.EmployeeId)
                    return ApiResponse<PayrollResponseDto>.Forbidden("You can only view your own payroll details.");

                if (payroll.Status == "DRAFT")
                    return ApiResponse<PayrollResponseDto>.Forbidden("You cannot view draft payroll details.");
            }

            return ApiResponse<PayrollResponseDto>.Ok(MapToPayrollDto(payroll));
        }

        public async Task<ApiResponse<PagedResult<PayrollResponseDto>>> GetPayrollsAsync(
            CurrentUser currentUser,
            PayrollFilterDto filter)
        {
            var query = _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Position)
                .Include(p => p.GeneratedByUser)
                .Include(p => p.ConfirmedByUser)
                .AsQueryable();

            if (filter.PayrollMonth > 0)
                query = query.Where(p => p.PayrollMonth == filter.PayrollMonth);

            if (filter.PayrollYear > 0)
                query = query.Where(p => p.PayrollYear == filter.PayrollYear);

            if (filter.EmployeeId.HasValue)
                query = query.Where(p => p.EmployeeId == filter.EmployeeId.Value);

            if (filter.DepartmentId.HasValue)
                query = query.Where(p => p.Employee.EmployeeAssignments.Any(ea => ea.DepartmentId == filter.DepartmentId.Value && ea.EndDate == null));

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(p => p.Status == filter.Status);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.PayrollId)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var dtos = items.Select(MapToPayrollDto).ToList();
            var result = PagedResult<PayrollResponseDto>.Create(dtos, filter.PageNumber, filter.PageSize, totalItems);

            return ApiResponse<PagedResult<PayrollResponseDto>>.Ok(result);
        }

        public async Task<ApiResponse<PayrollResponseDto>> UpdatePayrollDetailAsync(
            CurrentUser currentUser,
            int payrollId,
            UpdatePayrollDetailRequestDto request)
        {
            var payroll = await _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                .Include(p => p.PayrollDetails)
                .FirstOrDefaultAsync(p => p.PayrollId == payrollId);

            if (payroll == null)
                return ApiResponse<PayrollResponseDto>.NotFound("Payroll not found.");

            if (payroll.Status != "DRAFT")
                return ApiResponse<PayrollResponseDto>.Fail("Payroll details can only be updated for DRAFT payrolls.");

            // Validate all items
            var validTypes = new[] { "ALLOWANCE", "BONUS", "DEDUCTION", "OVERTIME", "OTHER" };
            foreach (var item in request.Items)
            {
                if (item.Amount < 0)
                    return ApiResponse<PayrollResponseDto>.Fail("Detail item amount cannot be negative.");

                if (!validTypes.Contains(item.ItemType.ToUpper()))
                    return ApiResponse<PayrollResponseDto>.Fail($"Invalid ItemType: '{item.ItemType}'. Valid types are: ALLOWANCE, BONUS, DEDUCTION, OVERTIME, OTHER.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Delete old details
                if (payroll.PayrollDetails != null && payroll.PayrollDetails.Any())
                {
                    _unitOfWork.PayrollDetails.DeleteRange(payroll.PayrollDetails);
                }

                // Add new details
                var newDetails = request.Items.Select(item => new PayrollDetail
                {
                    PayrollId = payrollId,
                    ItemType = item.ItemType.ToUpper(),
                    Description = item.Description,
                    Amount = item.Amount,
                    SourceType = item.SourceType,
                    SourceId = item.SourceId,
                    CreatedAt = DateTime.Now
                }).ToList();

                await _unitOfWork.PayrollDetails.AddRangeAsync(newDetails);

                // Recalculate totals
                payroll.TotalAllowance = newDetails.Where(d => d.ItemType == "ALLOWANCE").Sum(d => d.Amount);
                payroll.TotalBonus = newDetails.Where(d => d.ItemType == "BONUS").Sum(d => d.Amount);
                payroll.TotalOvertime = newDetails.Where(d => d.ItemType == "OVERTIME").Sum(d => d.Amount);
                payroll.TotalDeduction = newDetails.Where(d => d.ItemType == "DEDUCTION").Sum(d => d.Amount);

                payroll.GrossSalary = payroll.BaseSalary + payroll.TotalAllowance + payroll.TotalBonus + payroll.TotalOvertime;
                payroll.NetSalary = payroll.GrossSalary - payroll.TotalDeduction;
                if (payroll.NetSalary < 0) payroll.NetSalary = 0;

                _unitOfWork.Payrolls.Update(payroll);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<PayrollResponseDto>.Fail("Error updating payroll details: " + ex.Message);
            }

            // Reload and return
            var updated = await _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Position)
                .Include(p => p.PayrollDetails)
                .Include(p => p.GeneratedByUser)
                .Include(p => p.ConfirmedByUser)
                .FirstAsync(p => p.PayrollId == payrollId);

            return ApiResponse<PayrollResponseDto>.Ok(MapToPayrollDto(updated), "Payroll details updated successfully.");
        }

        public async Task<ApiResponse<bool>> ConfirmPayrollAsync(
            CurrentUser currentUser,
            ConfirmPayrollRequestDto request)
        {
            var query = _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                .Where(p => p.PayrollMonth == request.PayrollMonth &&
                            p.PayrollYear == request.PayrollYear &&
                            p.Status == "DRAFT");

            if (request.EmployeeId.HasValue)
            {
                query = query.Where(p => p.EmployeeId == request.EmployeeId.Value);
            }

            if (request.DepartmentId.HasValue)
            {
                query = query.Where(p => p.Employee.EmployeeAssignments.Any(ea => ea.DepartmentId == request.DepartmentId.Value && ea.EndDate == null));
            }

            var payrolls = await query.ToListAsync();

            if (!payrolls.Any())
                return ApiResponse<bool>.NotFound("No DRAFT payrolls found to confirm for this period.");

            foreach (var payroll in payrolls)
            {
                if (payroll.GrossSalary < 0 || payroll.NetSalary < 0)
                    return ApiResponse<bool>.Fail($"Payroll ID {payroll.PayrollId} has invalid calculations (negative gross/net salary).");

                payroll.Status = "CONFIRMED";
                payroll.ConfirmedByUserId = currentUser.UserId;
                payroll.ConfirmedAt = DateTime.Now;

                _unitOfWork.Payrolls.Update(payroll);
            }

            await _unitOfWork.SaveChangesAsync();

            // Try sending system notifications to employees
            var recipientUserIds = payrolls
                .Where(p => p.Employee?.UserId != null)
                .Select(p => p.Employee.UserId!.Value)
                .ToList();

            if (recipientUserIds.Any())
            {
                try
                {
                    await _notificationService.SendSystemNotificationAsync(
                        "Payslip Released",
                        $"Your payslip for {request.PayrollMonth}/{request.PayrollYear} is ready for viewing.",
                        "PAYROLL",
                        recipientUserIds
                    );
                }
                catch
                {
                    // Ignore notification failures so it doesn't break the main flow
                }
            }

            return ApiResponse<bool>.Ok(true, $"Confirmed {payrolls.Count} payrolls successfully.");
        }

        public async Task<ApiResponse<PayrollResponseDto>> GetMyPayslipAsync(
            CurrentUser currentUser,
            int payrollMonth,
            int payrollYear)
        {
            if (!currentUser.EmployeeId.HasValue)
                return ApiResponse<PayrollResponseDto>.Fail("Employee profile not found.");

            var employeeId = currentUser.EmployeeId.Value;

            var payroll = await _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Position)
                .Include(p => p.PayrollDetails)
                .Include(p => p.GeneratedByUser)
                .Include(p => p.ConfirmedByUser)
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId &&
                                          p.PayrollMonth == payrollMonth &&
                                          p.PayrollYear == payrollYear);

            if (payroll == null)
                return ApiResponse<PayrollResponseDto>.NotFound($"Payslip not found for period {payrollMonth}/{payrollYear}.");

            if (payroll.Status == "DRAFT")
                return ApiResponse<PayrollResponseDto>.Fail("Payslip is still in DRAFT status and is not yet visible.");

            return ApiResponse<PayrollResponseDto>.Ok(MapToPayrollDto(payroll));
        }

        public async Task<ApiResponse<PayrollReportDto>> ExportPayrollReportAsync(
            CurrentUser currentUser,
            PayrollFilterDto filter)
        {
            if (filter.PayrollMonth < 1 || filter.PayrollMonth > 12)
                return ApiResponse<PayrollReportDto>.Fail("Payroll month must be from 1 to 12.");

            if (filter.PayrollYear < 2000 || filter.PayrollYear > 2100)
                return ApiResponse<PayrollReportDto>.Fail("Payroll year must be valid.");

            var query = _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Position)
                .Include(p => p.PayrollDetails)
                .Include(p => p.GeneratedByUser)
                .Include(p => p.ConfirmedByUser)
                .Where(p => p.PayrollMonth == filter.PayrollMonth &&
                            p.PayrollYear == filter.PayrollYear &&
                            (p.Status == "CONFIRMED" || p.Status == "PAID"));

            if (filter.EmployeeId.HasValue)
            {
                query = query.Where(p => p.EmployeeId == filter.EmployeeId.Value);
            }

            if (filter.DepartmentId.HasValue)
            {
                query = query.Where(p => p.Employee.EmployeeAssignments.Any(ea => ea.DepartmentId == filter.DepartmentId.Value && ea.EndDate == null));
            }

            var payrolls = await query.ToListAsync();

            var report = new PayrollReportDto
            {
                PayrollMonth = filter.PayrollMonth,
                PayrollYear = filter.PayrollYear,
                TotalEmployees = payrolls.Count,
                TotalBaseSalary = payrolls.Sum(p => p.BaseSalary),
                TotalAllowance = payrolls.Sum(p => p.TotalAllowance),
                TotalBonus = payrolls.Sum(p => p.TotalBonus),
                TotalOvertime = payrolls.Sum(p => p.TotalOvertime),
                TotalDeduction = payrolls.Sum(p => p.TotalDeduction),
                TotalGrossSalary = payrolls.Sum(p => p.GrossSalary),
                TotalNetSalary = payrolls.Sum(p => p.NetSalary),
                Payrolls = payrolls.Select(MapToPayrollDto).ToList()
            };

            return ApiResponse<PayrollReportDto>.Ok(report, "Payroll report exported successfully.");
        }

        private PayrollResponseDto MapToPayrollDto(Payroll entity)
        {
            var assignment = entity.Employee?.EmployeeAssignments?.FirstOrDefault(ea => ea.EndDate == null);

            return new PayrollResponseDto
            {
                PayrollId = entity.PayrollId,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = entity.Employee?.EmployeeCode,
                EmployeeName = entity.Employee?.FullName,
                DepartmentName = assignment?.Department?.DepartmentName,
                PositionName = assignment?.Position?.PositionName,
                PayrollMonth = entity.PayrollMonth,
                PayrollYear = entity.PayrollYear,
                BaseSalary = entity.BaseSalary,
                TotalAllowance = entity.TotalAllowance,
                TotalBonus = entity.TotalBonus,
                TotalOvertime = entity.TotalOvertime,
                TotalDeduction = entity.TotalDeduction,
                GrossSalary = entity.GrossSalary,
                NetSalary = entity.NetSalary,
                Status = entity.Status,
                GeneratedByUserId = entity.GeneratedByUserId,
                GeneratedByUsername = entity.GeneratedByUser?.Username,
                ConfirmedByUserId = entity.ConfirmedByUserId,
                ConfirmedByUsername = entity.ConfirmedByUser?.Username,
                CreatedAt = entity.CreatedAt,
                ConfirmedAt = entity.ConfirmedAt,
                UpdatedAt = null,
                PayrollDetails = entity.PayrollDetails?.Select(d => new PayrollDetailResponseDto
                {
                    PayrollDetailId = d.PayrollDetailId,
                    PayrollId = d.PayrollId,
                    ItemType = d.ItemType,
                    Description = d.Description,
                    Amount = d.Amount,
                    SourceType = d.SourceType,
                    SourceId = d.SourceId,
                    CreatedAt = d.CreatedAt
                }).ToList() ?? new List<PayrollDetailResponseDto>()
            };
        }
    }
}