using HRM.Business.DTOs.Kpis;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/kpis")]
    [Authorize]
    public class KpisController : BaseApiController
    {
        private readonly IKpiService _kpiService;

        public KpisController(IKpiService kpiService)
        {
            _kpiService = kpiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetKpis([FromQuery] bool? isActive)
        {
            var result = await _kpiService.GetKpisAsync(isActive);
            return HandleResponse(result);
        }

        [HttpGet("{kpiId:int}")]
        public async Task<IActionResult> GetKpiById(int kpiId)
        {
            var result = await _kpiService.GetKpiByIdAsync(kpiId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "HR,Manager")]
        public async Task<IActionResult> CreateKpi([FromBody] CreateKpiRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _kpiService.CreateKpiAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("{kpiId:int}")]
        [Authorize(Roles = "HR,Manager")]
        public async Task<IActionResult> UpdateKpi(int kpiId, [FromBody] UpdateKpiRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _kpiService.UpdateKpiAsync(currentUser, kpiId, request);
            return HandleResponse(result);
        }

        [HttpPut("{kpiId:int}/deactivate")]
        [Authorize(Roles = "HR,Manager")]
        public async Task<IActionResult> DeactivateKpi(int kpiId)
        {
            var currentUser = GetCurrentUser();
            var result = await _kpiService.DeactivateKpiAsync(currentUser, kpiId);
            return HandleResponse(result);
        }

        [HttpPost("assignments")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> AssignKpi([FromBody] AssignKpiRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _kpiService.AssignKpiAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("assignments/{assignmentId:int}/progress")]
        public async Task<IActionResult> UpdateKpiProgress(
            int assignmentId,
            [FromBody] UpdateKpiProgressRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _kpiService.UpdateKpiProgressAsync(currentUser, assignmentId, request);
            return HandleResponse(result);
        }

        [HttpPut("assignments/{assignmentId:int}/evaluate")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EvaluateKpi(
            int assignmentId,
            [FromBody] EvaluateKpiRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _kpiService.EvaluateKpiAsync(currentUser, assignmentId, request);
            return HandleResponse(result);
        }

        [HttpGet("assignments/me")]
        public async Task<IActionResult> GetMyKpiAssignments([FromQuery] KpiAssignmentFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _kpiService.GetMyKpiAssignmentsAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("assignments")]
        [Authorize(Roles = "HR,Manager,Admin")]
        public async Task<IActionResult> GetKpiAssignments([FromQuery] KpiAssignmentFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _kpiService.GetKpiAssignmentsAsync(currentUser, filter);
            return HandleResponse(result);
        }
    }
}