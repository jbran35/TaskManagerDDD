using System.Net.Http.Headers;
using System.Security.Claims;

namespace TaskManager.Presentation
{
    public class AuthHeaderHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User.Identity?.IsAuthenticated == true)
            {
                var token = httpContext.User.FindFirstValue("jwt_token");

                if (!string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("String is not null or empty");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}