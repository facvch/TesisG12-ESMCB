using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace BlazorFrontEnd.Auth
{
    public class BlazorAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            // For Blazor component page requests (which return HTML), we want to let the request
            // go through to the rendering endpoint, so that Blazor's router (AuthorizeRouteView)
            // can handle the authorization inside the circuit using AuthenticationStateProvider.
            // This prevents crashes due to missing default authentication schemes on the server
            // and avoids breaking page refreshes for authenticated users.
            if (authorizeResult.Challenged || authorizeResult.Forbidden)
            {
                await next(context);
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
