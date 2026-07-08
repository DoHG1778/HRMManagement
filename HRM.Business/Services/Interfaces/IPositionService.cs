using HRM.Business.Common;
using HRM.Business.DTOs.Positions;

namespace HRM.Business.Services.Interfaces
{
    public interface IPositionService
    {
        Task<ApiResponse<List<PositionResponseDto>>> GetPositionsAsync(
            bool? isActive = null);

        Task<ApiResponse<PositionResponseDto>> GetPositionByIdAsync(
            int positionId);

        Task<ApiResponse<PositionResponseDto>> CreatePositionAsync(
            CurrentUser currentUser,
            CreatePositionRequestDto request);

        Task<ApiResponse<PositionResponseDto>> UpdatePositionAsync(
            CurrentUser currentUser,
            int positionId,
            UpdatePositionRequestDto request);

        Task<ApiResponse<bool>> DeactivatePositionAsync(
            CurrentUser currentUser,
            int positionId);

        Task<ApiResponse<bool>> ActivatePositionAsync(
            CurrentUser currentUser,
            int positionId);
    }
}