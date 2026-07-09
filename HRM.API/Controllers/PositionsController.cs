using HRM.Business.DTOs.Positions;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/positions")]
    [Authorize]
    public class PositionsController : BaseApiController
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator,Payroll,Payroll Officer")]
        public async Task<IActionResult> GetPositions([FromQuery] bool? isActive)
        {
            var result = await _positionService.GetPositionsAsync(isActive);
            return HandleResponse(result);
        }

        [HttpGet("{positionId:int}")]
        [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator,Payroll,Payroll Officer")]
        public async Task<IActionResult> GetPositionById(int positionId)
        {
            var result = await _positionService.GetPositionByIdAsync(positionId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> CreatePosition([FromBody] CreatePositionRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _positionService.CreatePositionAsync(currentUser, request);
            return HandleResponse(result);
        }

        [HttpPut("{positionId:int}")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> UpdatePosition(int positionId, [FromBody] UpdatePositionRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _positionService.UpdatePositionAsync(currentUser, positionId, request);
            return HandleResponse(result);
        }

        [HttpPut("{positionId:int}/deactivate")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> DeactivatePosition(int positionId)
        {
            var currentUser = GetCurrentUser();
            var result = await _positionService.DeactivatePositionAsync(currentUser, positionId);
            return HandleResponse(result);
        }

        [HttpPut("{positionId:int}/activate")]
        [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
        public async Task<IActionResult> ActivatePosition(int positionId)
        {
            var currentUser = GetCurrentUser();
            var result = await _positionService.ActivatePositionAsync(currentUser, positionId);
            return HandleResponse(result);
        }
    }
}
