using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Community.Application.Options;

namespace Community.Api.Security;

/// <summary>
/// Minimal gate for internal endpoints: the request is allowed when it carries
/// EITHER a valid JWT OR an <c>X-Service-Token</c> header matching the configured
/// <see cref="ServiceAuthOptions.Token"/> (service-to-service calls, e.g. the
/// Moderation service applying a Hide decision to a comment). Pair with
/// <c>[AllowAnonymous]</c> so authentication still populates the principal
/// without a controller-level <c>[Authorize]</c> rejecting a token-only caller.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ServiceOrUserAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>Allows the request through if it has an authenticated principal or a matching service token; otherwise returns 401.</summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        // Already authenticated via JWT: let it through as-is.
        if (http.User?.Identity?.IsAuthenticated ?? false)
        {
            await next();
            return;
        }

        var configuredToken = http.RequestServices
            .GetRequiredService<IOptions<ServiceAuthOptions>>().Value.Token;

        // Fall back to a matching X-Service-Token header for trusted service-to-service calls.
        if (!string.IsNullOrEmpty(configuredToken)
            && http.Request.Headers.TryGetValue(ServiceAuthConstants.HeaderName, out var provided)
            && FixedTimeEquals(provided.ToString(), configuredToken))
        {
            await next();
            return;
        }

        // Neither a valid user nor a valid service token: reject the request.
        context.Result = new UnauthorizedResult();
    }

    /// <summary>Constant-time string comparison to avoid leaking token length/content via timing.</summary>
    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
