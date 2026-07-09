using HRM.Business.Common;
using HRM.Business.DTOs.Employees;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "ACTIVE",
            "ON_LEAVE",
            "RESIGNED",
            "TERMINATED"
        };

        private readonly IUnitOfWork _unitOfWork;

        public EmployeeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PagedResult<EmployeeResponseDto>>> GetEmployeesAsync(
            CurrentUser currentUser,
            EmployeeFilterDto filter)
        {
            if (!CanViewEmployeeList(currentUser))
                return ApiResponse<PagedResult<EmployeeResponseDto>>.Forbidden("You are not allowed to view employee list.");

            filter ??= new EmployeeFilterDto();
            NormalizePaging(filter);

            var query = _unitOfWork.Employees.Query()
                .Include(e => e.Manager)
                .Include(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                    .ThenInclude(ea => ea.Department)
                .Include(e => e.EmployeeAssignments.Where(ea => ea.EndDate == null))
                    .ThenInclude(ea => ea.Position)
                .AsQueryable();

            if (IsManagerRole(currentUser) && !CanManageEmployeeData(currentUser))
            {
                if (!currentUser.EmployeeId.HasValue)
                    return ApiResponse<PagedResult<EmployeeResponseDto>>.Fail("Manager employee profile not found.");

                var managerEmployeeId = currentUser.EmployeeId.Value;
                query = query.Where(e =>
                    e.EmployeeAssignments.Any(ea =>
                        ea.EndDate == null &&
                        ea.Department.ManagerEmployeeId == managerEmployeeId));
            }

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();
                query = query.Where(e =>
                    e.EmployeeCode.Contains(keyword) ||
                    e.FullName.Contains(keyword) ||
                    e.Email.Contains(keyword));
            }

            if (filter.DepartmentId.HasValue)
            {
                query = query.Where(e =>
                    e.EmployeeAssignments.Any(ea =>
                        ea.EndDate == null &&
                        ea.DepartmentId == filter.DepartmentId.Value));
            }

            if (filter.PositionId.HasValue)
            {
                query = query.Where(e =>
                    e.EmployeeAssignments.Any(ea =>
                        ea.EndDate == null &&
                        ea.PositionId == filter.PositionId.Value));
            }

            if (!string.IsNullOrWhiteSpace(filter.EmploymentStatus))
            {
                var status = NormalizeStatus(filter.EmploymentStatus);
                query = query.Where(e => e.EmploymentStatus == status);
            }

            var totalItems = await query.CountAsync();
            var employees = await query
                .OrderBy(e => e.EmployeeCode)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = PagedResult<EmployeeResponseDto>.Create(
                employees.Select(MapEmployeeToDto).ToList(),
                filter.PageNumber,
                filter.PageSize,
                totalItems);

            return ApiResponse<PagedResult<EmployeeResponseDto>>.Ok(result);
        }

        public async Task<ApiResponse<EmployeeDetailResponseDto>> GetEmployeeDetailAsync(
            CurrentUser currentUser,
            int employeeId)
        {
            var employee = await _unitOfWork.Employees.GetEmployeeDetailAsync(employeeId);
            if (employee == null)
                return ApiResponse<EmployeeDetailResponseDto>.NotFound("Employee not found.");

            if (!await CanViewEmployeeDetailAsync(currentUser, employeeId))
                return ApiResponse<EmployeeDetailResponseDto>.Forbidden("You are not allowed to view this employee.");

            return ApiResponse<EmployeeDetailResponseDto>.Ok(MapEmployeeDetailToDto(employee));
        }

        public async Task<ApiResponse<EmployeeResponseDto>> CreateEmployeeAsync(
            CurrentUser currentUser,
            CreateEmployeeRequestDto request)
        {
            if (!CanManageEmployeeData(currentUser))
                return ApiResponse<EmployeeResponseDto>.Forbidden("You are not allowed to create employee profiles.");

            var validation = await ValidateCreateEmployeeAsync(request);
            if (!validation.Success)
                return validation;

            var employee = new Employee
            {
                EmployeeCode = request.EmployeeCode.Trim(),
                FullName = request.FullName.Trim(),
                Gender = request.Gender.Trim(),
                DateOfBirth = request.DateOfBirth,
                Phone = request.Phone,
                Email = request.Email.Trim(),
                Address = request.Address,
                Cccd = NormalizeOptional(request.Cccd),
                HireDate = request.HireDate,
                EmploymentStatus = "ACTIVE",
                UserId = request.UserId,
                ManagerId = request.ManagerId,
                AvatarUrl = request.AvatarUrl,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Employees.GetEmployeeDetailAsync(employee.EmployeeId);
            return ApiResponse<EmployeeResponseDto>.Ok(
                MapEmployeeToDto(created ?? employee),
                "Employee profile created successfully.",
                201);
        }

        public async Task<ApiResponse<EmployeeResponseDto>> UpdateEmployeeAsync(
            CurrentUser currentUser,
            int employeeId,
            UpdateEmployeeRequestDto request)
        {
            if (!CanManageEmployeeData(currentUser))
                return ApiResponse<EmployeeResponseDto>.Forbidden("You are not allowed to update employee profiles.");

            if (request == null)
                return ApiResponse<EmployeeResponseDto>.Fail("Employee information is required.");

            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
            if (employee == null)
                return ApiResponse<EmployeeResponseDto>.NotFound("Employee not found.");

            var validation = await ValidateUpdateEmployeeAsync(employee, request);
            if (!validation.Success)
                return validation;

            if (!string.IsNullOrWhiteSpace(request.FullName))
                employee.FullName = request.FullName.Trim();
            if (!string.IsNullOrWhiteSpace(request.Gender))
                employee.Gender = request.Gender.Trim();
            if (request.DateOfBirth.HasValue)
                employee.DateOfBirth = request.DateOfBirth.Value;
            if (request.Phone != null)
                employee.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Email))
                employee.Email = request.Email.Trim();
            if (request.Address != null)
                employee.Address = request.Address;
            if (request.Cccd != null)
                employee.Cccd = NormalizeOptional(request.Cccd);
            if (request.HireDate.HasValue)
                employee.HireDate = request.HireDate.Value;
            if (!string.IsNullOrWhiteSpace(request.EmploymentStatus))
                employee.EmploymentStatus = NormalizeStatus(request.EmploymentStatus);
            if (request.UserId.HasValue)
                employee.UserId = request.UserId.Value;
            if (request.ManagerId.HasValue)
                employee.ManagerId = request.ManagerId.Value;
            if (request.AvatarUrl != null)
                employee.AvatarUrl = request.AvatarUrl;

            employee.UpdatedAt = DateTime.Now;
            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Employees.GetEmployeeDetailAsync(employeeId);
            return ApiResponse<EmployeeResponseDto>.Ok(
                MapEmployeeToDto(updated ?? employee),
                "Employee profile updated successfully.");
        }

        public async Task<ApiResponse<EmployeeDetailResponseDto>> GetMyProfileAsync(
            CurrentUser currentUser)
        {
            var employee = await GetCurrentEmployeeAsync(currentUser);
            if (employee == null)
                return ApiResponse<EmployeeDetailResponseDto>.NotFound("Employee profile not found.");

            var detail = await _unitOfWork.Employees.GetEmployeeDetailAsync(employee.EmployeeId);
            return ApiResponse<EmployeeDetailResponseDto>.Ok(MapEmployeeDetailToDto(detail ?? employee));
        }

        public async Task<ApiResponse<EmployeeDetailResponseDto>> UpdateMyProfileAsync(
            CurrentUser currentUser,
            UpdateMyProfileRequestDto request)
        {
            if (request == null)
                return ApiResponse<EmployeeDetailResponseDto>.Fail("Profile information is required.");

            var employee = await GetCurrentEmployeeAsync(currentUser);
            if (employee == null)
                return ApiResponse<EmployeeDetailResponseDto>.NotFound("Employee profile not found.");

            if (request.Phone != null)
                employee.Phone = request.Phone;
            if (request.Address != null)
                employee.Address = request.Address;
            if (request.AvatarUrl != null)
                employee.AvatarUrl = request.AvatarUrl;

            employee.UpdatedAt = DateTime.Now;
            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Employees.GetEmployeeDetailAsync(employee.EmployeeId);
            return ApiResponse<EmployeeDetailResponseDto>.Ok(
                MapEmployeeDetailToDto(updated ?? employee),
                "My profile updated successfully.");
        }

        public async Task<ApiResponse<bool>> ChangeEmployeeStatusAsync(
            CurrentUser currentUser,
            int employeeId,
            string status)
        {
            if (!CanManageEmployeeData(currentUser))
                return ApiResponse<bool>.Forbidden("You are not allowed to change employee status.");

            if (string.IsNullOrWhiteSpace(status))
                return ApiResponse<bool>.Fail("Employment status is required.");

            var normalizedStatus = NormalizeStatus(status);
            if (!ValidStatuses.Contains(normalizedStatus))
                return ApiResponse<bool>.Fail("Employment status is invalid.");

            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
            if (employee == null)
                return ApiResponse<bool>.NotFound("Employee not found.");

            employee.EmploymentStatus = normalizedStatus;
            employee.UpdatedAt = DateTime.Now;
            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<bool>.Ok(true, "Employee status changed successfully.");
        }

        public async Task<ApiResponse<EmployeeAssignmentResponseDto>> AssignEmployeeAsync(
            CurrentUser currentUser,
            int employeeId,
            AssignEmployeeRequestDto request)
        {
            if (!CanManageEmployeeData(currentUser))
                return ApiResponse<EmployeeAssignmentResponseDto>.Forbidden("You are not allowed to assign employees.");

            var validation = await ValidateAssignmentInputAsync(employeeId, request?.DepartmentId ?? 0, request?.PositionId ?? 0, request?.StartDate ?? default);
            if (!validation.Success)
                return validation;

            if (await _unitOfWork.Employees.HasCurrentAssignmentAsync(employeeId))
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Employee already has a current assignment. Please use transfer employee.");

            var assignment = new EmployeeAssignment
            {
                EmployeeId = employeeId,
                DepartmentId = request!.DepartmentId,
                PositionId = request.PositionId,
                StartDate = request.StartDate,
                Note = request.Note,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Employees.AddAssignmentAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Employees.GetCurrentAssignmentAsync(employeeId);
            return ApiResponse<EmployeeAssignmentResponseDto>.Ok(
                MapAssignmentToDto(created ?? assignment),
                "Employee assigned successfully.",
                201);
        }

        public async Task<ApiResponse<EmployeeAssignmentResponseDto>> TransferEmployeeAsync(
            CurrentUser currentUser,
            int employeeId,
            TransferEmployeeRequestDto request)
        {
            if (!CanManageEmployeeData(currentUser))
                return ApiResponse<EmployeeAssignmentResponseDto>.Forbidden("You are not allowed to transfer employees.");

            var validation = await ValidateAssignmentInputAsync(employeeId, request?.DepartmentId ?? 0, request?.PositionId ?? 0, request?.StartDate ?? default);
            if (!validation.Success)
                return validation;

            var currentAssignment = await _unitOfWork.Employees.GetCurrentAssignmentAsync(employeeId);
            if (currentAssignment == null)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Employee does not have a current assignment.");

            if (request!.StartDate <= currentAssignment.StartDate)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Transfer start date must be greater than current assignment start date.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                currentAssignment.EndDate = request.StartDate.AddDays(-1);
                _unitOfWork.Employees.UpdateAssignment(currentAssignment);

                var newAssignment = new EmployeeAssignment
                {
                    EmployeeId = employeeId,
                    DepartmentId = request.DepartmentId,
                    PositionId = request.PositionId,
                    StartDate = request.StartDate,
                    Note = request.Note,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.Employees.AddAssignmentAsync(newAssignment);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var created = await _unitOfWork.Employees.GetCurrentAssignmentAsync(employeeId);
                return ApiResponse<EmployeeAssignmentResponseDto>.Ok(
                    MapAssignmentToDto(created ?? newAssignment),
                    "Employee transferred successfully.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<ApiResponse<EmployeeResponseDto>> ValidateCreateEmployeeAsync(CreateEmployeeRequestDto request)
        {
            if (request == null)
                return ApiResponse<EmployeeResponseDto>.Fail("Employee information is required.");

            if (string.IsNullOrWhiteSpace(request.EmployeeCode))
                return ApiResponse<EmployeeResponseDto>.Fail("Employee code is required.");
            if (string.IsNullOrWhiteSpace(request.FullName))
                return ApiResponse<EmployeeResponseDto>.Fail("Full name is required.");
            if (string.IsNullOrWhiteSpace(request.Gender))
                return ApiResponse<EmployeeResponseDto>.Fail("Gender is required.");
            if (string.IsNullOrWhiteSpace(request.Email))
                return ApiResponse<EmployeeResponseDto>.Fail("Email is required.");
            if (request.HireDate == default)
                return ApiResponse<EmployeeResponseDto>.Fail("Hire date is required.");

            if (await _unitOfWork.Employees.IsEmployeeCodeExistsAsync(request.EmployeeCode.Trim()))
                return ApiResponse<EmployeeResponseDto>.Fail("Employee code already exists.");
            if (await _unitOfWork.Employees.IsEmailExistsAsync(request.Email.Trim()))
                return ApiResponse<EmployeeResponseDto>.Fail("Employee email already exists.");
            if (!string.IsNullOrWhiteSpace(request.Cccd) && await _unitOfWork.Employees.IsCccdExistsAsync(request.Cccd.Trim()))
                return ApiResponse<EmployeeResponseDto>.Fail("CCCD already exists.");

            if (request.UserId.HasValue)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId.Value);
                if (user == null)
                    return ApiResponse<EmployeeResponseDto>.Fail("User account not found.");
                if (await _unitOfWork.Employees.IsUserLinkedToEmployeeAsync(request.UserId.Value))
                    return ApiResponse<EmployeeResponseDto>.Fail("User account is already linked to another employee.");
            }

            if (request.ManagerId.HasValue && !await _unitOfWork.Employees.IsActiveEmployeeAsync(request.ManagerId.Value))
                return ApiResponse<EmployeeResponseDto>.Fail("Manager must be an active employee.");

            return ApiResponse<EmployeeResponseDto>.Ok("Valid");
        }

        private async Task<ApiResponse<EmployeeResponseDto>> ValidateUpdateEmployeeAsync(Employee employee, UpdateEmployeeRequestDto request)
        {
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email.Trim();
                var emailExists = await _unitOfWork.Employees.Query()
                    .AnyAsync(e => e.EmployeeId != employee.EmployeeId && e.Email == email);
                if (emailExists)
                    return ApiResponse<EmployeeResponseDto>.Fail("Employee email already exists.");
            }

            if (request.Cccd != null && !string.IsNullOrWhiteSpace(request.Cccd))
            {
                var cccd = request.Cccd.Trim();
                var cccdExists = await _unitOfWork.Employees.Query()
                    .AnyAsync(e => e.EmployeeId != employee.EmployeeId && e.Cccd == cccd);
                if (cccdExists)
                    return ApiResponse<EmployeeResponseDto>.Fail("CCCD already exists.");
            }

            if (!string.IsNullOrWhiteSpace(request.EmploymentStatus))
            {
                var status = NormalizeStatus(request.EmploymentStatus);
                if (!ValidStatuses.Contains(status))
                    return ApiResponse<EmployeeResponseDto>.Fail("Employment status is invalid.");
            }

            if (request.UserId.HasValue)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(request.UserId.Value);
                if (user == null)
                    return ApiResponse<EmployeeResponseDto>.Fail("User account not found.");
                if (await _unitOfWork.Employees.IsUserLinkedToEmployeeAsync(request.UserId.Value, employee.EmployeeId))
                    return ApiResponse<EmployeeResponseDto>.Fail("User account is already linked to another employee.");
            }

            if (request.ManagerId.HasValue)
            {
                if (request.ManagerId.Value == employee.EmployeeId)
                    return ApiResponse<EmployeeResponseDto>.Fail("Employee cannot be their own manager.");
                if (!await _unitOfWork.Employees.IsActiveEmployeeAsync(request.ManagerId.Value))
                    return ApiResponse<EmployeeResponseDto>.Fail("Manager must be an active employee.");
            }

            return ApiResponse<EmployeeResponseDto>.Ok("Valid");
        }

        private async Task<ApiResponse<EmployeeAssignmentResponseDto>> ValidateAssignmentInputAsync(
            int employeeId,
            int departmentId,
            int positionId,
            DateOnly startDate)
        {
            if (departmentId <= 0)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Department is required.");
            if (positionId <= 0)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Position is required.");
            if (startDate == default)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Start date is required.");

            var employee = await _unitOfWork.Employees.GetByIdAsync(employeeId);
            if (employee == null)
                return ApiResponse<EmployeeAssignmentResponseDto>.NotFound("Employee not found.");
            if (employee.EmploymentStatus != "ACTIVE")
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Employee must be active.");
            if (startDate < employee.HireDate)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Start date cannot be earlier than hire date.");

            var department = await _unitOfWork.Departments.GetByIdAsync(departmentId);
            if (department == null)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Department not found.");
            if (!department.IsActive)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Department must be active.");

            var position = await _unitOfWork.Positions.GetByIdAsync(positionId);
            if (position == null)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Position not found.");
            if (!position.IsActive)
                return ApiResponse<EmployeeAssignmentResponseDto>.Fail("Position must be active.");

            return ApiResponse<EmployeeAssignmentResponseDto>.Ok("Valid");
        }

        private async Task<Employee?> GetCurrentEmployeeAsync(CurrentUser currentUser)
        {
            if (currentUser.EmployeeId.HasValue)
                return await _unitOfWork.Employees.GetByIdAsync(currentUser.EmployeeId.Value);

            if (currentUser.UserId > 0)
                return await _unitOfWork.Employees.GetByUserIdAsync(currentUser.UserId);

            return null;
        }

        private async Task<bool> CanViewEmployeeDetailAsync(CurrentUser currentUser, int employeeId)
        {
            if (CanManageEmployeeData(currentUser))
                return true;

            if (currentUser.EmployeeId == employeeId)
                return true;

            if (IsManagerRole(currentUser) && currentUser.EmployeeId.HasValue)
                return await _unitOfWork.Employees.IsEmployeeUnderManagerAsync(employeeId, currentUser.EmployeeId.Value);

            return false;
        }

        private static bool CanViewEmployeeList(CurrentUser currentUser)
        {
            return currentUser.HasAnyRole("Admin", "HR", "HR Staff", "Manager", "Department Manager", "System Administrator");
        }

        private static bool IsManagerRole(CurrentUser currentUser)
        {
            return currentUser.HasAnyRole("Manager", "Department Manager");
        }

        private static bool CanManageEmployeeData(CurrentUser currentUser)
        {
            return currentUser.HasAnyRole("Admin", "HR", "HR Staff", "System Administrator");
        }

        private static void NormalizePaging(EmployeeFilterDto filter)
        {
            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;
            if (filter.PageSize <= 0)
                filter.PageSize = 10;
            if (filter.PageSize > 100)
                filter.PageSize = 100;
        }

        private static string NormalizeStatus(string status)
        {
            return status.Trim().ToUpperInvariant();
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static EmployeeResponseDto MapEmployeeToDto(Employee employee)
        {
            return new EmployeeResponseDto
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                FullName = employee.FullName,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth,
                Phone = employee.Phone,
                Email = employee.Email,
                Address = employee.Address,
                Cccd = employee.Cccd,
                HireDate = employee.HireDate,
                EmploymentStatus = employee.EmploymentStatus,
                AvatarUrl = employee.AvatarUrl,
                UserId = employee.UserId,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager?.FullName
            };
        }

        private static EmployeeDetailResponseDto MapEmployeeDetailToDto(Employee employee)
        {
            return new EmployeeDetailResponseDto
            {
                EmployeeId = employee.EmployeeId,
                EmployeeCode = employee.EmployeeCode,
                FullName = employee.FullName,
                Gender = employee.Gender,
                DateOfBirth = employee.DateOfBirth,
                Phone = employee.Phone,
                Email = employee.Email,
                Address = employee.Address,
                Cccd = employee.Cccd,
                HireDate = employee.HireDate,
                EmploymentStatus = employee.EmploymentStatus,
                AvatarUrl = employee.AvatarUrl,
                UserId = employee.UserId,
                Username = employee.User?.Username,
                ManagerId = employee.ManagerId,
                ManagerName = employee.Manager?.FullName,
                DepartmentId = employee.EmployeeAssignment?.DepartmentId,
                DepartmentName = employee.EmployeeAssignment?.Department?.DepartmentName,
                PositionId = employee.EmployeeAssignment?.PositionId,
                PositionName = employee.EmployeeAssignment?.Position?.PositionName,
                CurrentSalary = employee.Contract?.Status == "ACTIVE" ? employee.Contract.Salary : null,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            };
        }

        private static EmployeeAssignmentResponseDto MapAssignmentToDto(EmployeeAssignment assignment)
        {
            return new EmployeeAssignmentResponseDto
            {
                AssignmentId = assignment.AssignmentId,
                EmployeeId = assignment.EmployeeId,
                EmployeeCode = assignment.Employee?.EmployeeCode ?? string.Empty,
                EmployeeName = assignment.Employee?.FullName ?? string.Empty,
                DepartmentId = assignment.DepartmentId,
                DepartmentName = assignment.Department?.DepartmentName ?? string.Empty,
                PositionId = assignment.PositionId,
                PositionName = assignment.Position?.PositionName ?? string.Empty,
                StartDate = assignment.StartDate,
                EndDate = assignment.EndDate,
                Note = assignment.Note,
                CreatedAt = assignment.CreatedAt
            };
        }
    }
}
