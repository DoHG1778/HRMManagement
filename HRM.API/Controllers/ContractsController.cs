using HRM.Business.DTOs.Contracts;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/contracts")]
    [Authorize]
    public class ContractsController : BaseApiController
    {
        private readonly IContractService _contractService;

        public ContractsController(IContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetContracts([FromQuery] ContractFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _contractService.GetContractsAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("{contractId:int}")]
        public async Task<IActionResult> GetContractById(int contractId)
        {
            var currentUser = GetCurrentUser();
            var result = await _contractService.GetContractByIdAsync(currentUser, contractId);
            return HandleResponse(result);
        }

        [HttpGet("employee/{employeeId:int}")]
        public async Task<IActionResult> GetContractsByEmployee(int employeeId)
        {
            var currentUser = GetCurrentUser();
            var result = await _contractService.GetContractsByEmployeeAsync(currentUser, employeeId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _contractService.CreateContractAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("{contractId:int}")]
        [Authorize(Roles = "Admin")] // UC14: CHỈ ADMIN MỚI ĐƯỢC PHÉP CẬP NHẬT
        public async Task<IActionResult> UpdateContract(int contractId, [FromBody] UpdateContractRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _contractService.UpdateContractAsync(currentUser, contractId, request);
            return HandleResponse(result);
        }

        [HttpPut("{contractId:int}/extend")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ExtendContract(int contractId, [FromBody] ExtendContractRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _contractService.ExtendContractAsync(currentUser, contractId, request);
            return HandleResponse(result);
        }

        [HttpPut("{contractId:int}/terminate")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> TerminateContract(int contractId, [FromBody] TerminateContractRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _contractService.TerminateContractAsync(currentUser, contractId, request);
            return HandleResponse(result);
        }

        [HttpGet("expiring")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetExpiringContracts([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        {
            var currentUser = GetCurrentUser();
            var result = await _contractService.GetExpiringContractsAsync(currentUser, fromDate, toDate);
            return HandleResponse(result);
        }
    }
}