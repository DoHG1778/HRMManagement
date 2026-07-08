using HRM.Business.DTOs.Attendances;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/attendances")]
    [Authorize]
    public class AttendancesController : BaseApiController
    {
        private readonly IAttendanceService _attendanceService;

        public AttendancesController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        [HttpPost("check-in")]
        [Authorize(Roles = "Employee,HR,Manager,Payroll")]
        public async Task<IActionResult> CheckIn()
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.CheckInAsync(currentUser);
            return HandleResponse(result);
        }

        [HttpPost("check-out")]
        [Authorize(Roles = "Employee,HR,Manager,Payroll")]
        public async Task<IActionResult> CheckOut()
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.CheckOutAsync(currentUser);
            return HandleResponse(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyAttendanceHistory([FromQuery] AttendanceFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.GetMyAttendanceHistoryAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR,Manager")]
        public async Task<IActionResult> GetAttendanceSheet([FromQuery] AttendanceFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.GetAttendanceSheetAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpPost("adjustments")]
        public async Task<IActionResult> CreateAdjustmentRequest([FromBody] CreateAttendanceAdjustmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.CreateAdjustmentRequestAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("adjustments/{adjustmentId:int}")]
        public async Task<IActionResult> UpdateAdjustmentRequest(
            int adjustmentId,
            [FromBody] UpdateAttendanceAdjustmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.UpdateAdjustmentRequestAsync(currentUser, adjustmentId, request);
            return HandleResponse(result);
        }

        [HttpPut("adjustments/{adjustmentId:int}/cancel")]
        public async Task<IActionResult> CancelAdjustmentRequest(int adjustmentId)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.CancelAdjustmentRequestAsync(currentUser, adjustmentId);
            return HandleResponse(result);
        }

        [HttpPut("adjustments/{adjustmentId:int}/approval")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ApproveOrRejectAdjustment(
            int adjustmentId,
            [FromBody] ApproveAttendanceAdjustmentRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.ApproveOrRejectAdjustmentAsync(currentUser, adjustmentId, request);
            return HandleResponse(result);
        }

        [HttpGet("adjustments/pending")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetPendingAdjustmentRequests()
        {
            var currentUser = GetCurrentUser();
            var result = await _attendanceService.GetPendingAdjustmentRequestsAsync(currentUser);
            return HandleResponse(result);
        }
    }
}