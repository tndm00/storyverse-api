using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Authentication.Application.Options;

namespace Authentication.Api.Security;

/// <summary>
/// Minimal gate for internal endpoints: the request is allowed when it carries
/// EITHER a valid JWT (an authenticated
/// <see cref="Microsoft.AspNetCore.Http.HttpContext.User"/>) OR an
/// <c>X-Service-Token</c> header matching the configured
/// <see cref="ServiceAuthOptions.Token"/> (service-to-service calls, e.g. the
/// Content service resolving an author's user id). Pair with
/// <c>[AllowAnonymous]</c> so authentication still populates the principal
/// without a controller-level <c>[Authorize]</c> rejecting a token-only caller.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ServiceOrUserAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        if (http.User?.Identity?.IsAuthenticated ?? false)
        {
            await next();
            return;
        }

        var configuredToken = http.RequestServices
            .GetRequiredService<IOptions<ServiceAuthOptions>>().Value.Token;

        if (!string.IsNullOrEmpty(configuredToken)
            && http.Request.Headers.TryGetValue(ServiceAuthConstants.HeaderName, out var provided)
            && FixedTimeEquals(provided.ToString(), configuredToken))
        {
            await next();
            return;
        }

        context.Result = new UnauthorizedResult();
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
