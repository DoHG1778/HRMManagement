using System.Text.Json;
using HRM.Business.Common;

namespace HRM.MVC.Helpers
{
    public static class ApiResponseReader
    {
        public static async Task<ApiResponse<T>> ReadAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return ApiResponse<T>.Fail(
                    $"Request failed with status {(int)response.StatusCode}.",
                    (int)response.StatusCode);
            }

            try
            {
                var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? ApiResponse<T>.Fail(
                    "Invalid response from API.",
                    (int)response.StatusCode);
            }
            catch
            {
                return ApiResponse<T>.Fail(
                    response.IsSuccessStatusCode ? "Invalid response from API." : $"API error: {content}",
                    (int)response.StatusCode);
            }
        }
    }
}
