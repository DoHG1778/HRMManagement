using HRM.Business.DTOs.Payrolls;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/payrolls")]
    [Authorize]
    public class PayrollsController : BaseApiController
    {
        private readonly IPayrollService _payrollService;

        public PayrollsController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        [HttpPost("generate")]
        [Authorize(Roles = "Payroll,Admin")]
        public async Task<IActionResult> GenerateMonthlyPayroll([FromBody] GeneratePayrollRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.GenerateMonthlyPayrollAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPost("calculate")]
        [Authorize(Roles = "Payroll,Admin")]
        public async Task<IActionResult> CalculatePayroll([FromBody] CalculatePayrollDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.CalculatePayrollAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpGet("{payrollId:int}")]
        [Authorize(Roles = "Employee,Payroll,Admin")]
        public async Task<IActionResult> GetPayrollDetail(int payrollId)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.GetPayrollDetailAsync(currentUser, payrollId);
            return HandleResponse(result);
        }

        [HttpGet]
        [Authorize(Roles = "Payroll,Admin")]
        public async Task<IActionResult> GetPayrolls([FromQuery] PayrollFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.GetPayrollsAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpPut("{payrollId:int}/details")]
        [Authorize(Roles = "Payroll,Admin")]
        public async Task<IActionResult> UpdatePayrollDetail(
            int payrollId,
            [FromBody] UpdatePayrollDetailRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.UpdatePayrollDetailAsync(currentUser, payrollId, request);
            return HandleResponse(result);
        }

        [HttpPut("confirm")]
        [Authorize(Roles = "Payroll,Admin")]
        public async Task<IActionResult> ConfirmPayroll([FromBody] ConfirmPayrollRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.ConfirmPayrollAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpGet("me")]
        [Authorize(Roles = "Employee,Payroll,Admin")]
        public async Task<IActionResult> GetMyPayslip([FromQuery] int payrollMonth, [FromQuery] int payrollYear)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.GetMyPayslipAsync(currentUser, payrollMonth, payrollYear);
            return HandleResponse(result);
        }

        [HttpGet("my-payslip")]
        [Authorize(Roles = "Employee,Payroll,Admin")]
        public async Task<IActionResult> GetMyPayslipExact([FromQuery] int month, [FromQuery] int year)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.GetMyPayslipAsync(currentUser, month, year);
            return HandleResponse(result);
        }

        [HttpGet("report")]
        [Authorize(Roles = "Payroll,Admin")]
        public async Task<IActionResult> ExportPayrollReport([FromQuery] PayrollFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _payrollService.ExportPayrollReportAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("export")]
        [Authorize(Roles = "Payroll,Admin")]
        public async Task<IActionResult> ExportPayrollReportExact(
            [FromQuery] int month,
            [FromQuery] int year,
            [FromQuery] int? departmentId,
            [FromQuery] int? employeeId)
        {
            var currentUser = GetCurrentUser();
            var filter = new PayrollFilterDto
            {
                PayrollMonth = month,
                PayrollYear = year,
                DepartmentId = departmentId,
                EmployeeId = employeeId
            };
            var result = await _payrollService.ExportPayrollReportAsync(currentUser, filter);
            return HandleResponse(result);
        }
    }
}