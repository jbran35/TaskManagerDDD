using System.Net.Http.Headers;
using System.Security.Claims;
using TaskManager.Presentation.Services;

namespace TaskManager.Presentation
{
    public class AuthHeaderHandler(TokenProviderService tokenProvider) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = tokenProvider.Token; 

            if (!string.IsNullOrEmpty(token))
            {

                Console.WriteLine("String is not null or empty");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}