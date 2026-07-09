using HRM.Business.DTOs.Departments;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/departments")]
    [Authorize]
    public class DepartmentsController : BaseApiController
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator,Payroll,Payroll Officer")]
        public async Task<IActionResult> GetDepartments([FromQuery] bool? isActive)
        {
            var result = await _departmentService.GetDepartmentsAsync(isActive);
            return HandleResponse(result);
        }

        [HttpGet("{departmentId:int}")]
        [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator,Payroll,Payroll Officer")]
        public async Task<IActionResult> GetDepartmentById(int departmentId)
        {
            var result = await _departmentService.GetDepartmentByIdAsync(departmentId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _departmentService.CreateDepartmentAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("{departmentId:int}")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, [FromBody] UpdateDepartmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _departmentService.UpdateDepartmentAsync(currentUser, departmentId, request);
            return HandleResponse(result);
        }

        [HttpPut("{departmentId:int}/deactivate")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> DeactivateDepartment(int departmentId)
        {
            var currentUser = GetCurrentUser();
            var result = await _departmentService.DeactivateDepartmentAsync(currentUser, departmentId);
            return HandleResponse(result);
        }

        [HttpPut("{departmentId:int}/activate")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> ActivateDepartment(int departmentId)
        {
            var currentUser = GetCurrentUser();
            var result = await _departmentService.ActivateDepartmentAsync(currentUser, departmentId);
            return HandleResponse(result);
        }
    }
}
