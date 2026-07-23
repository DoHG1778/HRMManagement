using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HRM.Razor.Pages.Notifications
{
    public class SendModel : PageModel
    {
        private readonly INotificationApiClient _notificationApiClient;
        private readonly IUserApiClient _userApiClient;

        public SendModel(
            INotificationApiClient notificationApiClient,
            IUserApiClient userApiClient)
        {
            _notificationApiClient = notificationApiClient;
            _userApiClient = userApiClient;
        }

        [BindProperty]
        public SendNotificationViewModel Notification { get; set; } = new();

        public List<UserItemModel> Users { get; set; } = new();

        public async Task OnGetAsync()
        {
            ViewData["ActivePage"] = "Notifications";

            var response =
                await _userApiClient.GetUsersAsync(
                    isActive: true,
                    pageNumber: 1,
                    pageSize: 100);

            if (response.Success && response.Data != null)
            {
                var currentUserId =
                    int.Parse(
                        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                Users = response.Data.Items
                    .Where(u => u.UserId != currentUserId)
                    .ToList();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "Notifications";

            if (!ModelState.IsValid)
            {
                await LoadUsersAsync();
                return Page();
            }

            var response =
                await _notificationApiClient
                    .SendNotificationAsync(Notification);

            if (!response.Success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    response.Message ?? "Gửi thông báo thất bại.");

                await LoadUsersAsync();
                return Page();
            }

            TempData["SuccessMessage"] =
                "Gửi thông báo thành công.";

            return RedirectToPage("/Notifications/Index");
        }

        private async Task LoadUsersAsync()
        {
            var response =
                await _userApiClient.GetUsersAsync(
                    isActive: true,
                    pageNumber: 1,
                    pageSize: 100);

            if (response.Success && response.Data != null)
            {
                var currentUserId =
                    int.Parse(
                        User.FindFirstValue(ClaimTypes.NameIdentifier)!);

                Users = response.Data.Items
                    .Where(u => u.UserId != currentUserId)
                    .ToList();
            }
        }
    }
}