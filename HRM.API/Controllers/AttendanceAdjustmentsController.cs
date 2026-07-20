using HRM.Business.DTOs.Attendances;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/attendance-adjustments")]
    [Authorize]
    public class AttendanceAdjustmentsController : BaseApiController
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceAdjustmentsController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdjustmentRequest([FromBody] CreateAttendanceAdjustmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.CreateAdjustmentRequestAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAdjustmentRequest(
            int id,
            [FromBody] UpdateAttendanceAdjustmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.UpdateAdjustmentRequestAsync(currentUser, id, request);
            return HandleResponse(result);
        }

        [HttpPut("{id:int}/cancel")]
        public async Task<IActionResult> CancelAdjustmentRequest(int id)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.CancelAdjustmentRequestAsync(currentUser, id);
            return HandleResponse(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAdjustmentDetail(int id)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.GetAdjustmentDetailAsync(currentUser, id);
            return HandleResponse(result);
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyAdjustmentRequests(
            [FromQuery] string? status,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.GetMyAdjustmentRequestsAsync(currentUser, status, month, year);
            return HandleResponse(result);
        }

        [HttpGet("available-attendances")]
        public async Task<IActionResult> GetAdjustableAttendances(
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.GetAdjustableAttendancesAsync(currentUser, month, year);
            return HandleResponse(result);
        }

        [HttpGet("pending")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPendingAdjustmentRequests(
            [FromQuery] int? employeeId,
            [FromQuery] int? month,
            [FromQuery] int? year)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.GetPendingAdjustmentRequestsAsync(currentUser, employeeId, month, year);
            return HandleResponse(result);
        }

        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveAdjustment(int id)
        {
            var currentUser = GetCurrentUser();
            var request = new ApproveAttendanceAdjustmentRequestDto { IsApproved = true };
            var result = await _attendanceService.ApproveOrRejectAdjustmentAsync(currentUser, id, request);
            return HandleResponse(result);
        }

        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> RejectAdjustment(
            int id,
            [FromBody] RejectAttendanceAdjustmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var approvalRequest = new ApproveAttendanceAdjustmentRequestDto 
            { 
                IsApproved = false, 
                RejectionReason = request.RejectionReason 
            };
            var result = await _attendanceService.ApproveOrRejectAdjustmentAsync(currentUser, id, approvalRequest);
            return HandleResponse(result);
        }
    }
}
