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
        public async Task<IActionResult> GetDepartments([FromQuery] bool? isActive)
        {
            var result = await _departmentService.GetDepartmentsAsync(isActive);
            return HandleResponse(result);
        }

        [HttpGet("{departmentId:int}")]
        public async Task<IActionResult> GetDepartmentById(int departmentId)
        {
            var result = await _departmentService.GetDepartmentByIdAsync(departmentId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _departmentService.CreateDepartmentAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("{departmentId:int}")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, [FromBody] UpdateDepartmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _departmentService.UpdateDepartmentAsync(currentUser, departmentId, request);
            return HandleResponse(result);
        }

        [HttpPut("{departmentId:int}/deactivate")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> DeactivateDepartment(int departmentId)
        {
            var currentUser = GetCurrentUser();
            var result = await _departmentService.DeactivateDepartmentAsync(currentUser, departmentId);
            return HandleResponse(result);
        }

        [HttpPut("{departmentId:int}/activate")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> ActivateDepartment(int departmentId)
        {
            var currentUser = GetCurrentUser();
            var result = await _departmentService.ActivateDepartmentAsync(currentUser, departmentId);
            return HandleResponse(result);
        }
    }
}