using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Notifications
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly INotificationApiClient _notificationApiClient;

        public IndexModel(
            INotificationApiClient notificationApiClient)
        {
            _notificationApiClient = notificationApiClient;
        }

        public HRM.Razor.Models.PagedResultModel<NotificationViewModel> Notifications
        {
            get;
            set;
        } = new();

        [BindProperty(SupportsGet = true)]
        public bool? IsRead { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? NotificationType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Notifications";

            var response =
                await _notificationApiClient.GetMyNotificationsAsync(
                    isRead: IsRead,
                    notificationType: NotificationType,
                    pageNumber: 1,
                    pageSize: 100
                );

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");

                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (response.Success && response.Data != null)
            {
                Notifications = response.Data;
            }

            return Page();
        }

        public async Task<IActionResult>
            OnPostMarkAsReadAsync(int notificationId)
        {
            var response =
                await _notificationApiClient.MarkAsReadAsync(
                    notificationId);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");

                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
            {
                TempData["SuccessMessage"] =
                    "Đã đánh dấu thông báo là đã đọc.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    response.Message ??
                    "Không thể đánh dấu thông báo.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult>
            OnPostDeleteAsync(int notificationId)
        {
            var response =
                await _notificationApiClient.DeleteNotificationAsync(
                    notificationId);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");

                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
            {
                TempData["SuccessMessage"] =
                    "Đã xóa thông báo.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    response.Message ??
                    "Không thể xóa thông báo.";
            }

            return RedirectToPage();
        }
    }
}