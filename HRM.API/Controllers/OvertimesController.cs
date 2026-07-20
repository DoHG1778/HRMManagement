using HRM.Business.DTOs.Overtimes;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/overtimes")]
    [Authorize]
    public class OvertimesController : BaseApiController
    {
        private readonly IOvertimeService _overtimeService;

        public OvertimesController(IOvertimeService overtimeService)
        {
            _overtimeService = overtimeService;
        }

        [HttpPost("requests")]
        public async Task<IActionResult> CreateOvertimeRequest([FromBody] CreateOvertimeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _overtimeService.CreateOvertimeRequestAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("requests/{overtimeRequestId:int}")]
        public async Task<IActionResult> UpdateOvertimeRequest(
            int overtimeRequestId,
            [FromBody] UpdateOvertimeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _overtimeService.UpdateOvertimeRequestAsync(currentUser, overtimeRequestId, request);
            return HandleResponse(result);
        }

        [HttpPut("requests/{overtimeRequestId:int}/cancel")]
        public async Task<IActionResult> CancelOvertimeRequest(int overtimeRequestId)
        {
            var currentUser = GetCurrentUser();
            var result = await _overtimeService.CancelOvertimeRequestAsync(currentUser, overtimeRequestId);
            return HandleResponse(result);
        }

        [HttpGet("requests/me")]
        public async Task<IActionResult> GetMyOvertimeRequests([FromQuery] OvertimeRequestFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _overtimeService.GetMyOvertimeRequestsAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> GetOvertimeRequests([FromQuery] OvertimeRequestFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _overtimeService.GetOvertimeRequestsAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("requests/pending")]
        public async Task<IActionResult> GetPendingOvertimeRequests()
        {
            var currentUser = GetCurrentUser();
            var result = await _overtimeService.GetPendingOvertimeRequestsAsync(currentUser);
            return HandleResponse(result);
        }

        [HttpPut("requests/{overtimeRequestId:int}/approval")]
        public async Task<IActionResult> ApproveOrRejectOvertimeRequest(
            int overtimeRequestId,
            [FromBody] ApproveOvertimeRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _overtimeService.ApproveOrRejectOvertimeRequestAsync(currentUser, overtimeRequestId, request);
            return HandleResponse(result);
        }

        [HttpGet("approved-for-payroll")]
        public async Task<IActionResult> GetApprovedOvertimeForPayroll(
            [FromQuery] int employeeId,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            var currentUser = GetCurrentUser();
            var result = await _overtimeService.GetApprovedOvertimeForPayrollAsync(currentUser, employeeId, month, year);
            return HandleResponse(result);
        }
    }
}