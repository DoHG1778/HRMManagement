using System.Net.Http.Json;
using System.Text.Json;
using HRM.Razor.Models;
using HRM.Razor.Services.Interfaces;

namespace HRM.Razor.Services.ApiClients
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Lỗi kết nối API: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data, _jsonOptions);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Lỗi kết nối API: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data, _jsonOptions);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Lỗi kết nối API: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponseAsync<T>(response);
            }
            catch (Exception ex)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = $"Lỗi kết nối API: {ex.Message}",
                    StatusCode = 500
                };
            }
        }

        private async Task<ApiResponse<T>> HandleResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, _jsonOptions);
                    if (result != null)
                    {
                        if (result.StatusCode == 0)
                        {
                            result.StatusCode = (int)response.StatusCode;
                        }
                        return result;
                    }
                }
                catch
                {
                    // Trường hợp content không đúng format ApiResponse<T>
                }
            }

            var statusCode = (int)response.StatusCode;
            var message = response.IsSuccessStatusCode
                ? "Thành công"
                : statusCode switch
                {
                    400 => "Yêu cầu không hợp lệ (400)",
                    401 => "Chưa đăng nhập hoặc phiên làm việc đã hết hạn (401)",
                    403 => "Bạn không có quyền truy cập chức năng này (403)",
                    404 => "Không tìm thấy dữ liệu yêu cầu (404)",
                    500 => "Lỗi hệ thống phía server API (500)",
                    _ => $"Yêu cầu thất bại với mã lỗi {statusCode}"
                };

            return new ApiResponse<T>
            {
                Success = response.IsSuccessStatusCode,
                Message = message,
                StatusCode = statusCode
            };
        }
    }
}
