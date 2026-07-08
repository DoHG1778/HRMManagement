using HRM.Business.Common;
using HRM.Business.DTOs.Contracts;

namespace HRM.Business.Services.Interfaces
{
    public interface IContractService
    {
        Task<ApiResponse<PagedResult<ContractResponseDto>>> GetContractsAsync(
            CurrentUser currentUser,
            ContractFilterDto filter);

        Task<ApiResponse<ContractResponseDto>> GetContractByIdAsync(
            CurrentUser currentUser,
            int contractId);

        Task<ApiResponse<List<ContractResponseDto>>> GetContractsByEmployeeAsync(
            CurrentUser currentUser,
            int employeeId);

        Task<ApiResponse<ContractResponseDto>> CreateContractAsync(
            CurrentUser currentUser,
            CreateContractRequestDto request);

        Task<ApiResponse<ContractResponseDto>> UpdateContractAsync(
            CurrentUser currentUser,
            int contractId,
            UpdateContractRequestDto request);

        Task<ApiResponse<ContractResponseDto>> ExtendContractAsync(
            CurrentUser currentUser,
            int contractId,
            ExtendContractRequestDto request);

        Task<ApiResponse<ContractResponseDto>> TerminateContractAsync(
            CurrentUser currentUser,
            int contractId,
            TerminateContractRequestDto request);

        Task<ApiResponse<List<ContractResponseDto>>> GetExpiringContractsAsync(
            CurrentUser currentUser,
            DateOnly fromDate,
            DateOnly toDate);
    }
}