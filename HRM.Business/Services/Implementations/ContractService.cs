using HRM.Business.Common;
using HRM.Business.DTOs.Contracts;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContractService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PagedResult<ContractResponseDto>>> GetContractsAsync(
            CurrentUser currentUser,
            ContractFilterDto filter)
        {
            var query = _unitOfWork.Contracts.Query()
                .Include(c => c.Employee)
                .AsQueryable();

            if (filter.EmployeeId.HasValue)
                query = query.Where(c => c.EmployeeId == filter.EmployeeId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(c => c.Status == filter.Status);

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var kw = filter.Keyword.Trim().ToLower();
                query = query.Where(c => c.Employee.FullName.ToLower().Contains(kw) || c.Employee.EmployeeCode.ToLower().Contains(kw));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var result = items.Select(MapToDto).ToList();
            var pagedResult = PagedResult<ContractResponseDto>.Create(result, filter.PageNumber, filter.PageSize, totalCount);

            return ApiResponse<PagedResult<ContractResponseDto>>.Ok(pagedResult);
        }

        public async Task<ApiResponse<ContractResponseDto>> GetContractByIdAsync(
            CurrentUser currentUser,
            int contractId)
        {
            var contract = await _unitOfWork.Contracts.Query()
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ContractId == contractId);

            if (contract == null) return ApiResponse<ContractResponseDto>.NotFound();

            return ApiResponse<ContractResponseDto>.Ok(MapToDto(contract));
        }

        public async Task<ApiResponse<List<ContractResponseDto>>> GetContractsByEmployeeAsync(
            CurrentUser currentUser,
            int employeeId)
        {
            var contracts = await _unitOfWork.Contracts.Query()
                .Where(c => c.EmployeeId == employeeId)
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            return ApiResponse<List<ContractResponseDto>>.Ok(contracts.Select(MapToDto).ToList());
        }

        public async Task<ApiResponse<ContractResponseDto>> CreateContractAsync(
            CurrentUser currentUser,
            CreateContractRequestDto request)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId);
            if (employee == null) return ApiResponse<ContractResponseDto>.Fail("Employee not found.");

            var hasActiveContract = await _unitOfWork.Contracts.Query()
                .AnyAsync(c => c.EmployeeId == request.EmployeeId && c.Status == "ACTIVE");
            
            if (hasActiveContract)
                return ApiResponse<ContractResponseDto>.Fail("This employee already has an active contract.");

            var contract = new Contract
            {
                EmployeeId = request.EmployeeId,
                ContractType = request.ContractType,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Salary = request.Salary,
                Status = "ACTIVE",
                Note = request.Note,
                ContractFileUrl = request.ContractFileUrl,
                CreatedAt = DateTime.Now
            };

            await _unitOfWork.Contracts.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            var result = await _unitOfWork.Contracts.Query()
                .Include(c => c.Employee)
                .FirstOrDefaultAsync(c => c.ContractId == contract.ContractId);

            return ApiResponse<ContractResponseDto>.Ok(MapToDto(result!), "Contract created successfully.");
        }

        public async Task<ApiResponse<ContractResponseDto>> UpdateContractAsync(
            CurrentUser currentUser,
            int contractId,
            UpdateContractRequestDto request)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
            if (contract == null) return ApiResponse<ContractResponseDto>.NotFound();

            // Sửa lỗi CS0266 bằng cách gán tường minh
            if (request.ContractType != null)
                contract.ContractType = request.ContractType;
            
            if (request.StartDate.HasValue)
                contract.StartDate = request.StartDate.Value;
            
            contract.EndDate = request.EndDate; // Cho phép null nên gán trực tiếp
            
            if (request.Salary.HasValue)
                contract.Salary = request.Salary.Value;
            
            if (request.Note != null)
                contract.Note = request.Note;
            
            if (request.ContractFileUrl != null)
                contract.ContractFileUrl = request.ContractFileUrl;

            contract.UpdatedAt = DateTime.Now;

            _unitOfWork.Contracts.Update(contract);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<ContractResponseDto>.Ok(MapToDto(contract), "Contract updated successfully.");
        }

        public async Task<ApiResponse<ContractResponseDto>> ExtendContractAsync(
            CurrentUser currentUser,
            int contractId,
            ExtendContractRequestDto request)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
            if (contract == null) return ApiResponse<ContractResponseDto>.NotFound();

            contract.EndDate = request.NewEndDate;
            contract.UpdatedAt = DateTime.Now;
            
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<ContractResponseDto>.Ok(MapToDto(contract), "Contract extended successfully.");
        }

        public async Task<ApiResponse<ContractResponseDto>> TerminateContractAsync(
            CurrentUser currentUser,
            int contractId,
            TerminateContractRequestDto request)
        {
            var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId);
            if (contract == null) return ApiResponse<ContractResponseDto>.NotFound();

            contract.Status = "TERMINATED";
            contract.EndDate = request.TerminationDate;
            contract.UpdatedAt = DateTime.Now;
            
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<ContractResponseDto>.Ok(MapToDto(contract), "Contract terminated successfully.");
        }

        public async Task<ApiResponse<List<ContractResponseDto>>> GetExpiringContractsAsync(
            CurrentUser currentUser,
            DateOnly fromDate,
            DateOnly toDate)
        {
            var contracts = await _unitOfWork.Contracts.Query()
                .Include(c => c.Employee)
                .Where(c => c.Status == "ACTIVE" && c.EndDate.HasValue 
                            && c.EndDate.Value >= fromDate && c.EndDate.Value <= toDate)
                .ToListAsync();

            return ApiResponse<List<ContractResponseDto>>.Ok(contracts.Select(MapToDto).ToList());
        }

        private ContractResponseDto MapToDto(Contract c)
        {
            return new ContractResponseDto
            {
                ContractId = c.ContractId,
                EmployeeId = c.EmployeeId,
                EmployeeName = c.Employee?.FullName ?? "N/A",
                EmployeeCode = c.Employee?.EmployeeCode ?? "N/A",
                ContractType = c.ContractType,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Salary = c.Salary,
                Status = c.Status,
                Note = c.Note,
                ContractFileUrl = c.ContractFileUrl,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }
}