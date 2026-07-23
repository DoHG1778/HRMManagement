using HRM.Business.Common;
using HRM.Razor.Models.ViewModels;

namespace HRM.Razor.Services.Interfaces
{
    public interface IPositionApiClient
    {
        Task<ApiResponse<List<PositionModel>>> GetPositionsAsync(bool? isActive = null);

        Task<ApiResponse<PositionModel>> GetPositionByIdAsync(int positionId);

        Task<ApiResponse<PositionModel>> CreatePositionAsync(CreatePositionViewModel model);

        Task<ApiResponse<PositionModel>> UpdatePositionAsync(int positionId, EditPositionViewModel model);

        Task<ApiResponse<bool>> DeactivatePositionAsync(int positionId);

        Task<ApiResponse<bool>> ActivatePositionAsync(int positionId);
    }
}
