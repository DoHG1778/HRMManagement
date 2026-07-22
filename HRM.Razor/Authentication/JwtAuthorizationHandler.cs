using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace HRM.Razor.Authentication
{
    public class JwtAuthorizationHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JwtAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null && httpContext.Session != null)
            {
                var requestPath = request.RequestUri?.AbsolutePath ?? string.Empty;
                var isLoginRequest = requestPath.EndsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase);

                if (!isLoginRequest)
                {
                    var token = httpContext.Session.GetString("JWToken");
                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
