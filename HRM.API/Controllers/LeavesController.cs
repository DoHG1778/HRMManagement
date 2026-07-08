using HRM.Business.DTOs.Leaves;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/leaves")]
    [Authorize]
    public class LeavesController : BaseApiController
    {
        private readonly ILeaveService _leaveService;

        public LeavesController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpPost("requests")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] CreateLeaveRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.CreateLeaveRequestAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("requests/{leaveRequestId:int}")]
        public async Task<IActionResult> UpdateLeaveRequest(
            int leaveRequestId,
            [FromBody] UpdateLeaveRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.UpdateLeaveRequestAsync(currentUser, leaveRequestId, request);
            return HandleResponse(result);
        }

        [HttpPut("requests/{leaveRequestId:int}/cancel")]
        public async Task<IActionResult> CancelLeaveRequest(int leaveRequestId)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.CancelLeaveRequestAsync(currentUser, leaveRequestId);
            return HandleResponse(result);
        }

        [HttpGet("requests/me")]
        public async Task<IActionResult> GetMyLeaveRequests([FromQuery] LeaveRequestFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.GetMyLeaveRequestsAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("requests")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> GetLeaveRequests([FromQuery] LeaveRequestFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.GetLeaveRequestsAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("requests/pending")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPendingLeaveRequests()
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.GetPendingLeaveRequestsAsync(currentUser);
            return HandleResponse(result);
        }

        [HttpPut("requests/{leaveRequestId:int}/approval")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveOrRejectLeaveRequest(
            int leaveRequestId,
            [FromBody] ApproveLeaveRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.ApproveOrRejectLeaveRequestAsync(currentUser, leaveRequestId, request);
            return HandleResponse(result);
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetLeaveTypes([FromQuery] bool? isActive)
        {
            var result = await _leaveService.GetLeaveTypesAsync(isActive);
            return HandleResponse(result);
        }

        [HttpPost("types")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> CreateLeaveType([FromBody] CreateLeaveTypeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.CreateLeaveTypeAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("types/{leaveTypeId:int}")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> UpdateLeaveType(
            int leaveTypeId,
            [FromBody] UpdateLeaveTypeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.UpdateLeaveTypeAsync(currentUser, leaveTypeId, request);
            return HandleResponse(result);
        }

        [HttpPut("types/{leaveTypeId:int}/deactivate")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> DeactivateLeaveType(int leaveTypeId)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.DeactivateLeaveTypeAsync(currentUser, leaveTypeId);
            return HandleResponse(result);
        }

        [HttpPost("balances")]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> SetLeaveBalance([FromBody] SetLeaveBalanceRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.SetLeaveBalanceAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpGet("balances/employee/{employeeId:int}")]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> GetLeaveBalancesByEmployee(int employeeId, [FromQuery] int? year)
        {
            var currentUser = GetCurrentUser();
            var result = await _leaveService.GetLeaveBalancesByEmployeeAsync(currentUser, employeeId, year);
            return HandleResponse(result);
        }
    }
}