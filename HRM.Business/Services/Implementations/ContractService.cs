using HRM.Business.Common;
using HRM.Business.DTOs.Contracts;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

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
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<ContractResponseDto>> GetContractByIdAsync(
            CurrentUser currentUser,
            int contractId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<ContractResponseDto>>> GetContractsByEmployeeAsync(
            CurrentUser currentUser,
            int employeeId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<ContractResponseDto>> CreateContractAsync(
            CurrentUser currentUser,
            CreateContractRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<ContractResponseDto>> UpdateContractAsync(
            CurrentUser currentUser,
            int contractId,
            UpdateContractRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<ContractResponseDto>> ExtendContractAsync(
            CurrentUser currentUser,
            int contractId,
            ExtendContractRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<ContractResponseDto>> TerminateContractAsync(
            CurrentUser currentUser,
            int contractId,
            TerminateContractRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<List<ContractResponseDto>>> GetExpiringContractsAsync(
            CurrentUser currentUser,
            DateOnly fromDate,
            DateOnly toDate)
        {
            throw new NotImplementedException();
        }
    }
}