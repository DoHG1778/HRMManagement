using HRM.Business.Common;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly IApiClient _apiClient;

        public AuthApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginViewModel model)
        {
            var requestData = new
            {
                UsernameOrEmail = model.UsernameOrEmail?.Trim() ?? string.Empty,
                Password = model.Password
            };

            return await _apiClient.PostAsync<LoginResponseModel>("api/auth/login", requestData);
        }

        public async Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            var requestData = new
            {
                CurrentPassword = model.CurrentPassword,
                NewPassword = model.NewPassword,
                ConfirmPassword = model.ConfirmPassword
            };

            return await _apiClient.PostAsync<bool>("api/auth/change-password", requestData);
        }
    }
}
