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
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> GetEmployees([FromQuery] EmployeeFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.GetEmployeesAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("{employeeId:int}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> GetEmployeeDetail(int employeeId)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.GetEmployeeDetailAsync(currentUser, employeeId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.CreateEmployeeAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("{employeeId:int}")]
        [Authorize(Roles = "Admin,HR")]
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
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ChangeEmployeeStatus(int employeeId, [FromQuery] string status)
        {
            var currentUser = GetCurrentUser();
            var result = await _employeeService.ChangeEmployeeStatusAsync(currentUser, employeeId, status);
            return HandleResponse(result);
        }
    }
}