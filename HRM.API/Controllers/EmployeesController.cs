using HRM.Business.DTOs.Employees;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/employees")]
    [Authorize]
    public class EmployeesController : BaseApiController
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator")]
        public async Task<IActionResult> GetEmployees([FromQuery] EmployeeFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.GetEmployeesAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("{employeeId:int}")]
        [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator")]
        public async Task<IActionResult> GetEmployeeDetail(int employeeId)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.GetEmployeeDetailAsync(currentUser, employeeId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.CreateEmployeeAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("{employeeId:int}")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> UpdateEmployee(int employeeId, [FromBody] UpdateEmployeeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.UpdateEmployeeAsync(currentUser, employeeId, request);
            return HandleResponse(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.GetMyProfileAsync(currentUser);
            return HandleResponse(result);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateMyProfileRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.UpdateMyProfileAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPatch("{employeeId:int}/status")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> ChangeEmployeeStatus(int employeeId, [FromQuery] string status)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.ChangeEmployeeStatusAsync(currentUser, employeeId, status);
            return HandleResponse(result);
        }

        [HttpPost("{employeeId:int}/assignment")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> AssignEmployee(int employeeId, [FromBody] AssignEmployeeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.AssignEmployeeAsync(currentUser, employeeId, request);
            return HandleResponse(result);
        }

        [HttpPost("{employeeId:int}/transfer")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> TransferEmployee(int employeeId, [FromBody] TransferEmployeeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.TransferEmployeeAsync(currentUser, employeeId, request);
            return HandleResponse(result);
        }

        [HttpGet("{employeeId:int}/assignments")]
        [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator")]
        public async Task<IActionResult> GetAssignmentHistory(int employeeId)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.GetAssignmentHistoryAsync(currentUser, employeeId);
            return HandleResponse(result);
        }
    }
}
