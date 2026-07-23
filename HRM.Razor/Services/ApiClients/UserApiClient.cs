using HRM.Business.Common;
using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class UserApiClient : IUserApiClient
    {
        private readonly IApiClient _apiClient;

        public UserApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<
            ApiResponse<
                HRM.Razor.Models.PagedResultModel<UserItemModel>
            >
        >
        GetUsersAsync(
            string? keyword = null,
            bool? isActive = true,
            int pageNumber = 1,
            int pageSize = 100)
        {
            var queryParams = new List<string>
            {
                $"pageNumber={pageNumber}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                queryParams.Add(
                    $"keyword={Uri.EscapeDataString(keyword.Trim())}");
            }

            if (isActive.HasValue)
            {
                queryParams.Add(
                    $"isActive={isActive.Value.ToString().ToLower()}");
            }

            var endpoint =
                $"api/users?{string.Join("&", queryParams)}";

            return await _apiClient.GetAsync<
                HRM.Razor.Models.PagedResultModel<UserItemModel>
            >(endpoint);
        }
    }
}