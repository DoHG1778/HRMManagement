using HRM.Business.DTOs.Notifications;
using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : BaseApiController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationFilterDto filter)
        {
            var currentUser = GetCurrentUser();
            var result = await _notificationService.GetMyNotificationsAsync(currentUser, filter);
            return HandleResponse(result);
        }

        [HttpGet("{notificationId:int}")]
        public async Task<IActionResult> GetNotificationDetail(int notificationId)
        {
            var currentUser = GetCurrentUser();
            var result = await _notificationService.GetNotificationDetailAsync(currentUser, notificationId);
            return HandleResponse(result);
        }

        [HttpPut("{notificationId:int}/read")]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var currentUser = GetCurrentUser();
            var result = await _notificationService.MarkAsReadAsync(currentUser, notificationId);
            return HandleResponse(result);
        }

        [HttpPut("{notificationId:int}/delete")]
        public async Task<IActionResult> DeleteNotification(int notificationId)
        {
            var currentUser = GetCurrentUser();
            var result = await _notificationService.DeleteNotificationAsync(currentUser, notificationId);
            return HandleResponse(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,HR")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequestDto request)
        {
            var currentUser = GetCurrentUser();
            var result = await _notificationService.SendNotificationAsync(currentUser, request);
            return HandleResponse(result);
        }
    }
}