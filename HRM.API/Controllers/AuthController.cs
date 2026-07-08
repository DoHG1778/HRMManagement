using HRM.Business.DTOs.Auth;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/auth")]
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return HandleResponse(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _authService.ChangePasswordAsync(currentUser.UserId, request);
            return HandleResponse(result);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMyAccount()
        {
            var currentUser = GetCurrentUser();
            var result = await _authService.GetMyAccountAsync(currentUser.UserId);
            return HandleResponse(result);
        }
    }
}