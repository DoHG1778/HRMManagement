using HRM.Business.Common;
using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels;

namespace HRM.Razor.Services.Interfaces
{
    public interface IUserApiClient
    {
        Task<ApiResponse<HRM.Razor.Models.PagedResultModel<UserItemModel>>>
            GetUsersAsync(
                string? keyword = null,
                bool? isActive = true,
                int pageNumber = 1,
                int pageSize = 100);
    }
}