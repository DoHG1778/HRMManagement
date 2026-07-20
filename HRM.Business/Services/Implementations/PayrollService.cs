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

            var periodStart = new DateTime(request.PayrollYear, request.PayrollMonth, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);
            var periodStartOnly = DateOnly.FromDateTime(periodStart);
            var periodEndOnly = DateOnly.FromDateTime(periodEnd);

            // Fetch contracts valid in this period
            var validPeriodContracts = await _unitOfWork.Contracts.Query()
                .Where(c => c.StartDate <= periodEndOnly && (c.EndDate == null || c.EndDate >= periodStartOnly))
                .ToListAsync();

            var validEmployeeIds = validPeriodContracts.Select(c => c.EmployeeId).Distinct().ToList();

            // Get employees
            var employeeQuery = _unitOfWork.Employees.Query()
                .Include(e => e.EmployeeAssignments)
                    .ThenInclude(ea => ea.Department)
                .Include(e => e.EmployeeAssignments)
                    .ThenInclude(ea => ea.Position)
                .Where(e => e.EmploymentStatus == "ACTIVE");

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
            var skippedEmployees = new List<string>();

            foreach (var emp in activeEmployees)
            {
                if (existingEmployeeIds.Contains(emp.EmployeeId))
                    continue;

                var empContracts = validPeriodContracts.Where(c => c.EmployeeId == emp.EmployeeId).ToList();
                if (!empContracts.Any())
                {
                    skippedEmployees.Add(emp.FullName);
                    continue;
                }

                // Choose most recent contract starting in or before period
                var contract = empContracts.OrderByDescending(c => c.StartDate).First();

                var periodAssignment = emp.EmployeeAssignments
                    .Where(ea => ea.StartDate <= periodEndOnly && (ea.EndDate == null || ea.EndDate >= periodStartOnly))
                    .OrderByDescending(ea => ea.StartDate)
                    .FirstOrDefault()
                    ?? emp.EmployeeAssignment;

                var payroll = new Payroll
                {
                    EmployeeId = emp.EmployeeId,
                    PayrollMonth = request.PayrollMonth,
                    PayrollYear = request.PayrollYear,
                    BaseSalary = contract.Salary,
                    TotalAllowance = 0,
                    TotalBonus = 0,
                    TotalOvertime = 0,
                    TotalDeduction = 0,
                    GrossSalary = contract.Salary,
                    NetSalary = contract.Salary,
                    Status = "DRAFT",
                    GeneratedByUserId = currentUser.UserId,
                    CreatedAt = DateTime.Now
                };
                newPayrolls.Add(payroll);
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (newPayrolls.Any())
                {
                    await _unitOfWork.Payrolls.AddRangeAsync(newPayrolls);
                    await _unitOfWork.SaveChangesAsync();
                }
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<List<PayrollResponseDto>>.Fail("Error generating payrolls: " + ex.Message);
            }

            // Retrieve all payrolls for the period to return
            var query = _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Position)
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

            string msg = $"Generated payrolls successfully for period {request.PayrollMonth}/{request.PayrollYear}.";
            if (skippedEmployees.Any())
            {
                msg += $" (Skipped {skippedEmployees.Count} employee(s) without valid contract: {string.Join(", ", skippedEmployees)})";
            }

            return ApiResponse<List<PayrollResponseDto>>.Ok(dtos, msg);
        }

        public async Task<ApiResponse<List<PayrollResponseDto>>> CalculatePayrollAsync(
            CurrentUser currentUser,
            CalculatePayrollDto request)
        {
            if (request.DefaultAllowance < 0 || request.DefaultBonus < 0 || request.DefaultDeduction < 0)
                return ApiResponse<List<PayrollResponseDto>>.Fail("Allowance, bonus, and deduction must be non-negative.");

            if (request.OvertimeCoefficient <= 0 || request.StandardWorkingDays <= 0 || request.StandardWorkingHoursPerDay <= 0)
                return ApiResponse<List<PayrollResponseDto>>.Fail("Overtime coefficient, standard working days, and standard working hours per day must be positive.");

            var periodStart = new DateTime(request.PayrollYear, request.PayrollMonth, 1);
            var periodEnd = periodStart.AddMonths(1).AddDays(-1);
            var periodStartOnly = DateOnly.FromDateTime(periodStart);
            var periodEndOnly = DateOnly.FromDateTime(periodEnd);

            var query = _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                .Include(p => p.PayrollDetails)
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
            {
                return ApiResponse<List<PayrollResponseDto>>.Fail("No DRAFT payrolls found to calculate for this period.");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var payroll in payrolls)
                {
                    // 1. Get base salary from contract valid in this period
                    var empContracts = await _unitOfWork.Contracts.Query()
                        .Where(c => c.EmployeeId == payroll.EmployeeId &&
                                     c.StartDate <= periodEndOnly &&
                                     (c.EndDate == null || c.EndDate >= periodStartOnly))
                        .ToListAsync();
                    if (empContracts.Any())
                    {
                        var contract = empContracts.OrderByDescending(c => c.StartDate).First();
                        payroll.BaseSalary = contract.Salary;
                    }

                    // 2. Clear old automatic details
                    if (payroll.PayrollDetails != null && payroll.PayrollDetails.Any())
                    {
                        var autoDetails = payroll.PayrollDetails.Where(d => d.SourceType == "SYSTEM" || d.SourceType == "OVERTIME").ToList();
                        if (autoDetails.Any())
                        {
                            _unitOfWork.PayrollDetails.DeleteRange(autoDetails);
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
                    decimal hourlySalary = payroll.BaseSalary / request.StandardWorkingDays / request.StandardWorkingHoursPerDay;

                    foreach (var ot in otRequests)
                    {
                        decimal otAmount = Math.Round(hourlySalary * request.OvertimeCoefficient * ot.TotalHours, 2);
                        if (otAmount > 0)
                        {
                            newDetails.Add(new PayrollDetail
                            {
                                PayrollId = payroll.PayrollId,
                                ItemType = "OVERTIME",
                                Amount = otAmount,
                                Description = $"Approved Overtime on {ot.Otdate:yyyy-MM-dd} ({ot.TotalHours} hrs @ Coefficient {request.OvertimeCoefficient})",
                                SourceType = "OVERTIME",
                                SourceId = ot.Otid,
                                CreatedAt = DateTime.Now
                            });
                            totalOvertime += otAmount;
                        }
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
                            SourceType = "SYSTEM",
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
                            SourceType = "SYSTEM",
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
                            SourceType = "SYSTEM",
                            CreatedAt = DateTime.Now
                        });
                    }

                    if (newDetails.Any())
                    {
                        await _unitOfWork.PayrollDetails.AddRangeAsync(newDetails);
                    }

                    // 5. Update payroll totals using all details (new automatic + existing manual)
                    var allDetails = newDetails.Concat((payroll.PayrollDetails ?? new List<PayrollDetail>()).Where(d => d.SourceType == "MANUAL")).ToList();

                    payroll.TotalAllowance = allDetails.Where(d => d.ItemType == "ALLOWANCE").Sum(d => d.Amount);
                    payroll.TotalBonus = allDetails.Where(d => d.ItemType == "BONUS").Sum(d => d.Amount);
                    payroll.TotalOvertime = allDetails.Where(d => d.ItemType == "OVERTIME").Sum(d => d.Amount);
                    payroll.TotalDeduction = allDetails.Where(d => d.ItemType == "DEDUCTION").Sum(d => d.Amount);

                    decimal otherEarning = allDetails.Where(d => d.ItemType == "OTHER").Sum(d => d.Amount);

                    payroll.GrossSalary = payroll.BaseSalary + payroll.TotalAllowance + payroll.TotalBonus + payroll.TotalOvertime + otherEarning;
                    payroll.NetSalary = payroll.GrossSalary - payroll.TotalDeduction;
                    if (payroll.NetSalary < 0) payroll.NetSalary = 0;

                    payroll.Status = "DRAFT";
                    payroll.UpdatedAt = DateTime.Now;

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
            var updatedQuery = _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Department)
                .Include(p => p.Employee)
                    .ThenInclude(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                        .ThenInclude(ea => ea.Position)
                .Include(p => p.PayrollDetails)
                .Include(p => p.GeneratedByUser)
                .Include(p => p.ConfirmedByUser)
                .Where(p => p.PayrollMonth == request.PayrollMonth &&
                            p.PayrollYear == request.PayrollYear &&
                            p.Status == "DRAFT");

            if (request.EmployeeId.HasValue)
            {
                updatedQuery = updatedQuery.Where(p => p.EmployeeId == request.EmployeeId.Value);
            }

            if (request.DepartmentId.HasValue)
            {
                updatedQuery = updatedQuery.Where(p => p.Employee.EmployeeAssignments.Any(ea => ea.DepartmentId == request.DepartmentId.Value && ea.EndDate == null));
            }

            var updatedPayrolls = await updatedQuery.ToListAsync();
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
            if (!currentUser.IsAdmin() && !currentUser.IsPayroll())
            {
                if (currentUser.IsEmployee() && payroll.EmployeeId == currentUser.EmployeeId)
                {
                    if (payroll.Status != "CONFIRMED" && payroll.Status != "PAID")
                    {
                        return ApiResponse<PayrollResponseDto>.Forbidden("You cannot view draft payroll details.");
                    }
                }
                else
                {
                    return ApiResponse<PayrollResponseDto>.Forbidden("You do not have permission to view this payroll.");
                }
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
                if (string.IsNullOrWhiteSpace(item.Description))
                    return ApiResponse<PayrollResponseDto>.Fail("Detail item description cannot be empty.");

                if (item.Amount <= 0)
                    return ApiResponse<PayrollResponseDto>.Fail("Detail item amount must be positive.");

                if (item.Amount > 1000000000000m)
                    return ApiResponse<PayrollResponseDto>.Fail("Detail item amount exceeds maximum allowed value.");

                if (!validTypes.Contains(item.ItemType.ToUpper()))
                    return ApiResponse<PayrollResponseDto>.Fail($"Invalid ItemType: '{item.ItemType}'. Valid types are: ALLOWANCE, BONUS, DEDUCTION, OVERTIME, OTHER.");
            }

            // Protect automatic overtime and system details
            var existingDetails = payroll.PayrollDetails.ToList();
            var existingAutoDetails = existingDetails.Where(d => d.SourceType == "SYSTEM" || d.SourceType == "OVERTIME").ToList();
            var existingManualDetails = existingDetails.Where(d => d.SourceType == "MANUAL").ToList();

            foreach (var autoDetail in existingAutoDetails)
            {
                var matched = request.Items.FirstOrDefault(item =>
                    item.ItemType.Equals(autoDetail.ItemType, StringComparison.OrdinalIgnoreCase) &&
                    (item.Description ?? "").Equals(autoDetail.Description ?? "", StringComparison.OrdinalIgnoreCase) &&
                    item.Amount == autoDetail.Amount &&
                    (item.SourceType ?? "").Equals(autoDetail.SourceType ?? "", StringComparison.OrdinalIgnoreCase) &&
                    item.SourceId == autoDetail.SourceId);

                if (matched == null)
                {
                    return ApiResponse<PayrollResponseDto>.Fail("Automatic system and overtime details cannot be modified or deleted manually. Please recalculate payroll instead.");
                }
            }

            // Prevent tampering / client forging auto details
            foreach (var item in request.Items)
            {
                bool isAuto = item.SourceType == "SYSTEM" || item.SourceType == "OVERTIME";
                if (isAuto)
                {
                    var match = existingAutoDetails.FirstOrDefault(autoDetail =>
                        item.ItemType.Equals(autoDetail.ItemType, StringComparison.OrdinalIgnoreCase) &&
                        (item.Description ?? "").Equals(autoDetail.Description ?? "", StringComparison.OrdinalIgnoreCase) &&
                        item.Amount == autoDetail.Amount &&
                        (item.SourceType ?? "").Equals(autoDetail.SourceType ?? "", StringComparison.OrdinalIgnoreCase) &&
                        item.SourceId == autoDetail.SourceId);

                    if (match == null)
                    {
                        return ApiResponse<PayrollResponseDto>.Fail("You are not allowed to assign SYSTEM or OVERTIME source types, or assign a SourceId manually.");
                    }
                }
            }

            string initialStatus = payroll.Status;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Delete old manual details
                if (existingManualDetails.Any())
                {
                    _unitOfWork.PayrollDetails.DeleteRange(existingManualDetails);
                }

                // Prepare manual details from request
                var newManualDetails = new List<PayrollDetail>();
                foreach (var item in request.Items)
                {
                    bool isAuto = existingAutoDetails.Any(autoDetail =>
                        item.ItemType.Equals(autoDetail.ItemType, StringComparison.OrdinalIgnoreCase) &&
                        (item.Description ?? "").Equals(autoDetail.Description ?? "", StringComparison.OrdinalIgnoreCase) &&
                        item.Amount == autoDetail.Amount &&
                        (item.SourceType ?? "").Equals(autoDetail.SourceType ?? "", StringComparison.OrdinalIgnoreCase) &&
                        item.SourceId == autoDetail.SourceId);

                    if (!isAuto)
                    {
                        newManualDetails.Add(new PayrollDetail
                        {
                            PayrollId = payrollId,
                            ItemType = item.ItemType.ToUpper(),
                            Description = item.Description,
                            Amount = item.Amount,
                            SourceType = "MANUAL",
                            SourceId = null,
                            CreatedAt = DateTime.Now
                        });
                    }
                }

                if (newManualDetails.Any())
                {
                    await _unitOfWork.PayrollDetails.AddRangeAsync(newManualDetails);
                }

                var newDetails = existingAutoDetails.Concat(newManualDetails).ToList();

                // Recalculate totals
                payroll.TotalAllowance = newDetails.Where(d => d.ItemType == "ALLOWANCE").Sum(d => d.Amount);
                payroll.TotalBonus = newDetails.Where(d => d.ItemType == "BONUS").Sum(d => d.Amount);
                payroll.TotalOvertime = newDetails.Where(d => d.ItemType == "OVERTIME").Sum(d => d.Amount);
                payroll.TotalDeduction = newDetails.Where(d => d.ItemType == "DEDUCTION").Sum(d => d.Amount);

                decimal otherEarning = newDetails.Where(d => d.ItemType == "OTHER").Sum(d => d.Amount);

                payroll.GrossSalary = payroll.BaseSalary + payroll.TotalAllowance + payroll.TotalBonus + payroll.TotalOvertime + otherEarning;
                payroll.NetSalary = payroll.GrossSalary - payroll.TotalDeduction;
                if (payroll.NetSalary < 0) payroll.NetSalary = 0;

                payroll.Status = initialStatus;
                payroll.UpdatedAt = DateTime.Now;

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
                .Include(p => p.PayrollDetails)
                .Where(p => p.PayrollMonth == request.PayrollMonth &&
                            p.PayrollYear == request.PayrollYear);

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
                return ApiResponse<bool>.NotFound("No payrolls found to confirm for this period and filters.");

            foreach (var payroll in payrolls)
            {
                if (payroll.Status == "CONFIRMED" || payroll.Status == "PAID")
                {
                    continue; // Skip already confirmed/paid in bulk
                }

                if (payroll.Status != "DRAFT")
                {
                    return ApiResponse<bool>.Fail($"Payroll for Employee {payroll.Employee?.FullName ?? payroll.EmployeeId.ToString()} has invalid status '{payroll.Status}'. Only DRAFT payrolls can be confirmed.");
                }

                if (payroll.UpdatedAt == null)
                {
                    return ApiResponse<bool>.Fail("Payroll has not been calculated. Please calculate payroll first.");
                }

                if (payroll.GrossSalary <= 0 || payroll.NetSalary < 0)
                {
                    return ApiResponse<bool>.Fail($"Payroll for Employee {payroll.Employee?.FullName ?? payroll.EmployeeId.ToString()} has invalid calculated values (Gross: {payroll.GrossSalary}, Net: {payroll.NetSalary}). Please Calculate Payroll first.");
                }

                // Check details totals match
                decimal totalEarnings = payroll.PayrollDetails
                    .Where(d => d.ItemType == "ALLOWANCE" || d.ItemType == "BONUS" || d.ItemType == "OVERTIME" || d.ItemType == "OTHER")
                    .Sum(d => d.Amount);

                decimal totalDeductions = payroll.PayrollDetails
                    .Where(d => d.ItemType == "DEDUCTION")
                    .Sum(d => d.Amount);

                decimal expectedGross = payroll.BaseSalary + totalEarnings;
                decimal expectedNet = expectedGross - totalDeductions;
                if (expectedNet < 0) expectedNet = 0;

                if (Math.Abs(expectedGross - payroll.GrossSalary) > 0.02m)
                {
                    return ApiResponse<bool>.Fail($"Payroll Gross Salary ({payroll.GrossSalary}) does not match Base Salary and details sum ({expectedGross}) for Employee {payroll.Employee?.FullName}. Please Recalculate.");
                }

                if (Math.Abs(expectedNet - payroll.NetSalary) > 0.02m)
                {
                    return ApiResponse<bool>.Fail($"Payroll Net Salary ({payroll.NetSalary}) does not match Gross and deductions sum ({expectedNet}) for Employee {payroll.Employee?.FullName}. Please Recalculate.");
                }

                // Check for new approved OT requests not included in Payroll Details
                var approvedOts = await _unitOfWork.OvertimeRequests.Query()
                    .Where(ot => ot.EmployeeId == payroll.EmployeeId &&
                                 ot.Status == "APPROVED" &&
                                 !ot.IsTransferredToPayroll &&
                                 ot.Otdate.Month == payroll.PayrollMonth &&
                                 ot.Otdate.Year == payroll.PayrollYear)
                    .ToListAsync();

                var calculatedOtIds = payroll.PayrollDetails
                    .Where(d => d.SourceType == "OVERTIME" && d.SourceId.HasValue)
                    .Select(d => d.SourceId!.Value)
                    .ToList();

                var hasNewOt = approvedOts.Any(ot => !calculatedOtIds.Contains(ot.Otid));
                if (hasNewOt)
                {
                    return ApiResponse<bool>.Fail($"New approved Overtime requests exist for Employee {payroll.Employee?.FullName} that are not in the payroll details. Please Recalculate Payroll first.");
                }
            }

            var confirmedCount = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var payroll in payrolls)
                {
                    if (payroll.Status != "DRAFT")
                        continue;

                    payroll.Status = "CONFIRMED";
                    payroll.ConfirmedByUserId = currentUser.UserId;
                    payroll.ConfirmedAt = DateTime.Now;
                    payroll.UpdatedAt = DateTime.Now;

                    _unitOfWork.Payrolls.Update(payroll);
                    confirmedCount++;

                    // Mark only OT requests that actually exist in details
                    var otIdsToTransfer = payroll.PayrollDetails
                        .Where(d => d.SourceType == "OVERTIME" && d.SourceId.HasValue)
                        .Select(d => d.SourceId!.Value)
                        .ToList();

                    if (otIdsToTransfer.Any())
                    {
                        var otsToUpdate = await _unitOfWork.OvertimeRequests.Query()
                            .Where(ot => otIdsToTransfer.Contains(ot.Otid))
                            .ToListAsync();

                        foreach (var ot in otsToUpdate)
                        {
                            ot.IsTransferredToPayroll = true;
                            _unitOfWork.OvertimeRequests.Update(ot);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<bool>.Fail("Error confirming payroll: " + ex.Message);
            }

            // Try sending system notifications to employees
            if (confirmedCount > 0)
            {
                var recipientUserIds = payrolls
                    .Where(p => p.Status == "CONFIRMED" && p.Employee?.UserId != null)
                    .Select(p => p.Employee.UserId!.Value)
                    .Distinct()
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
            }

            return ApiResponse<bool>.Ok(true, $"Confirmed {confirmedCount} payrolls successfully.");
        }

        public async Task<ApiResponse<bool>> PayPayrollAsync(
            CurrentUser currentUser,
            ConfirmPayrollRequestDto request)
        {
            if (!currentUser.IsAdmin() && !currentUser.IsPayroll())
            {
                return ApiResponse<bool>.Forbidden("You do not have permission to mark payrolls as paid.");
            }

            var query = _unitOfWork.Payrolls.Query()
                .Include(p => p.Employee)
                .Where(p => p.PayrollMonth == request.PayrollMonth &&
                            p.PayrollYear == request.PayrollYear);

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
                return ApiResponse<bool>.NotFound("No payrolls found to pay for this period and filters.");

            var paidCount = 0;
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var payroll in payrolls)
                {
                    if (payroll.Status != "CONFIRMED")
                        continue;

                    payroll.Status = "PAID";
                    payroll.UpdatedAt = DateTime.Now;

                    _unitOfWork.Payrolls.Update(payroll);
                    paidCount++;
                }

                if (paidCount == 0)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return ApiResponse<bool>.Fail("No CONFIRMED payrolls found to mark as paid for this period and filters.");
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<bool>.Fail("Error marking payroll as paid: " + ex.Message);
            }

            return ApiResponse<bool>.Ok(true, $"Marked {paidCount} payroll(s) as paid successfully.");
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

            if (payroll.Status != "CONFIRMED" && payroll.Status != "PAID")
                return ApiResponse<PayrollResponseDto>.Fail("Payslip is not available.");

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
                UpdatedAt = entity.UpdatedAt,
                PayrollDetails = entity.PayrollDetails?.Select(d => new PayrollDetailResponseDto
                {
                    PayrollDetailId = d.PayrollDetailId,
                    PayrollId = d.PayrollId,
                    ItemType = d.ItemType,
                    Description = d.Description,
                    Amount = d.Amount,
                    SourceType = d.SourceType,
                    SourceId = d.SourceId,
                    IsManual = d.SourceType == "MANUAL",
                    CreatedAt = d.CreatedAt
                }).ToList() ?? new List<PayrollDetailResponseDto>()
            };
        }
    }
}