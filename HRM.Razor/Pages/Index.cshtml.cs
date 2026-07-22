using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _apiClient;

        public IndexModel(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public string ApiStatusMessage { get; set; } = "Đang kiểm tra kết nối API...";
        public bool IsApiConnected { get; set; }

        public async Task OnGetAsync()
        {
            ViewData["ActivePage"] = "Dashboard";
            
            // Gọi kiểm tra API kết nối tới HRM.API
            var response = await _apiClient.GetAsync<object>("api/dashboard");
            IsApiConnected = response.StatusCode != 500 && response.StatusCode != 0;
            
            if (IsApiConnected)
            {
                ApiStatusMessage = "Đã kết nối thành công tới HRM.API (Port 7100).";
            }
            else
            {
                ApiStatusMessage = $"Không thể kết nối HRM.API: {response.Message}";
            }
        }
    }
}
