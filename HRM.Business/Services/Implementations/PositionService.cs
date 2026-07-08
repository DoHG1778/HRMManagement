using HRM.Business.Common;
using HRM.Business.DTOs.Positions;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class PositionService : IPositionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PositionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<List<PositionResponseDto>>> GetPositionsAsync(
            bool? isActive = null)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PositionResponseDto>> GetPositionByIdAsync(
            int positionId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PositionResponseDto>> CreatePositionAsync(
            CurrentUser currentUser,
            CreatePositionRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<PositionResponseDto>> UpdatePositionAsync(
            CurrentUser currentUser,
            int positionId,
            UpdatePositionRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> DeactivatePositionAsync(
            CurrentUser currentUser,
            int positionId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> ActivatePositionAsync(
            CurrentUser currentUser,
            int positionId)
        {
            throw new NotImplementedException();
        }
    }
}