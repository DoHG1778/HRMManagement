using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class NotificationApiClient : INotificationApiClient
    {
        private readonly IApiClient _apiClient;

        public NotificationApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<
            ApiResponse<
                HRM.Razor.Models.PagedResultModel<NotificationViewModel>
            >
        >
        GetMyNotificationsAsync(
            bool? isRead = null,
            string? notificationType = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (isRead.HasValue)
            {
                queryParams.Add(
                    $"isRead={isRead.Value.ToString().ToLower()}");
            }

            if (!string.IsNullOrWhiteSpace(notificationType))
            {
                queryParams.Add(
                    $"notificationType={Uri.EscapeDataString(
                        notificationType.Trim())}");
            }

            var endpoint =
                $"api/notifications/me?{string.Join("&", queryParams)}";

            return await _apiClient.GetAsync<
                HRM.Razor.Models.PagedResultModel<NotificationViewModel>
            >(endpoint);
        }

        public async Task<ApiResponse<NotificationViewModel>>
            GetNotificationDetailAsync(int notificationId)
        {
            return await _apiClient.GetAsync<NotificationViewModel>(
                $"api/notifications/{notificationId}");
        }

        public async Task<ApiResponse<bool>>
            MarkAsReadAsync(int notificationId)
        {
            return await _apiClient.PutAsync<bool>(
                $"api/notifications/{notificationId}/read",
                new { });
        }

        public async Task<ApiResponse<bool>>
            DeleteNotificationAsync(int notificationId)
        {
            return await _apiClient.PutAsync<bool>(
                $"api/notifications/{notificationId}/delete",
                new { });
        }

        public async Task<ApiResponse<NotificationViewModel>>
            SendNotificationAsync(
                SendNotificationViewModel model)
        {
            var requestData = new
            {
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                NotificationType = model.NotificationType.Trim(),
                TargetType = model.TargetType.Trim(),
                UserIds = model.UserIds,
                EmployeeIds = model.EmployeeIds,
                DepartmentId = model.DepartmentId
            };

            return await _apiClient.PostAsync<NotificationViewModel>(
                "api/notifications",
                requestData);
        }
    }
}