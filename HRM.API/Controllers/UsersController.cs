using HRM.Business.DTOs.Users;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/users")]
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? keyword,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetUsersAsync(keyword, isActive, pageNumber, pageSize);
            return HandleResponse(result);
        }

        [HttpGet("{userId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var result = await _userService.GetUserByIdAsync(userId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _userService.CreateUserAsync(currentUser.UserId, request);
            return HandleResponse(result);
        }

        [HttpPut("{userId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _userService.UpdateUserAsync(currentUser.UserId, userId, request);
            return HandleResponse(result);
        }

        [HttpPut("{userId:int}/lock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockUser(int userId)
        {
            var currentUser = GetCurrentUser();
            var result = await _userService.LockUserAsync(currentUser.UserId, userId);
            return HandleResponse(result);
        }

        [HttpPut("{userId:int}/unlock")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnlockUser(int userId)
        {
            var currentUser = GetCurrentUser();
            var result = await _userService.UnlockUserAsync(currentUser.UserId, userId);
            return HandleResponse(result);
        }

        [HttpPost("assign-roles")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRoleRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _userService.AssignRolesAsync(currentUser.UserId, request);
            return HandleResponse(result);
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> GetRoles()
        {
            var result = await _userService.GetRolesAsync();
            return HandleResponse(result);
        }
    }
}