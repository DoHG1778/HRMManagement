using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels;

namespace HRM.Razor.Services.Interfaces
{
    public interface IAuthApiClient
    {
        Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginViewModel model);
        Task<ApiResponse<bool>> ChangePasswordAsync(ChangePasswordViewModel model);
    }
}
