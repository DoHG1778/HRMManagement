using HRM.Business.Common;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class PositionApiClient : IPositionApiClient
    {
        private readonly IApiClient _apiClient;

        public PositionApiClient(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<ApiResponse<List<PositionModel>>> GetPositionsAsync(bool? isActive = null)
        {
            var endpoint = isActive.HasValue
                ? $"api/positions?isActive={isActive.Value.ToString().ToLower()}"
                : "api/positions";
            return await _apiClient.GetAsync<List<PositionModel>>(endpoint);
        }

        public async Task<ApiResponse<PositionModel>> GetPositionByIdAsync(int positionId)
        {
            return await _apiClient.GetAsync<PositionModel>($"api/positions/{positionId}");
        }

        public async Task<ApiResponse<PositionModel>> CreatePositionAsync(CreatePositionViewModel model)
        {
            var requestData = new
            {
                PositionName = model.PositionName?.Trim() ?? string.Empty,
                Description = model.Description?.Trim()
            };
            return await _apiClient.PostAsync<PositionModel>("api/positions", requestData);
        }

        public async Task<ApiResponse<PositionModel>> UpdatePositionAsync(int positionId, EditPositionViewModel model)
        {
            var requestData = new
            {
                PositionName = model.PositionName?.Trim(),
                Description = model.Description?.Trim(),
                IsActive = model.IsActive
            };
            return await _apiClient.PutAsync<PositionModel>($"api/positions/{positionId}", requestData);
        }

        public async Task<ApiResponse<bool>> DeactivatePositionAsync(int positionId)
        {
            return await _apiClient.PutAsync<bool>($"api/positions/{positionId}/deactivate", new { });
        }

        public async Task<ApiResponse<bool>> ActivatePositionAsync(int positionId)
        {
            return await _apiClient.PutAsync<bool>($"api/positions/{positionId}/activate", new { });
        }
    }
}
