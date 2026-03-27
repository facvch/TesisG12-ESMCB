using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace BlazorFrontEnd.Auth
{
    public class JwtAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly TokenStorageService _tokenStorage;

        public JwtAuthorizationMessageHandler(ILocalStorageService localStorage, TokenStorageService tokenStorage)
        {
            _localStorage = localStorage;
            _tokenStorage = tokenStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string? token = null;

            // Try localStorage first (works during interactive rendering)
            try
            {
                token = await _localStorage.GetItemAsync<string>("authToken");
            }
            catch (InvalidOperationException)
            {
                // JS Interop not available during static rendering — use in-memory fallback
            }

            // Fallback to in-memory token storage
            if (string.IsNullOrWhiteSpace(token))
            {
                token = _tokenStorage.Token;
            }

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
