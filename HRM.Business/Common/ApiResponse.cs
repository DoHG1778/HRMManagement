namespace HRM.Business.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public T? Data { get; set; }

        public List<string> Errors { get; set; } = new();

        public int StatusCode { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Success", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> Ok(string message = "Success", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> Fail(string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> Fail(List<string> errors, string message = "Failed", int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized")
        {
            return Fail(message, 401);
        }

        public static ApiResponse<T> Forbidden(string message = "Forbidden")
        {
            return Fail(message, 403);
        }

        public static ApiResponse<T> NotFound(string message = "Not found")
        {
            return Fail(message, 404);
        }
    }
}