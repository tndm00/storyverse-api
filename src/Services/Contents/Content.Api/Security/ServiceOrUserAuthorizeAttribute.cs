using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Content.Application.Options;

namespace Content.Api.Security;

/// <summary>
/// Minimal gate for internal endpoints: the request is allowed when it carries
/// EITHER a valid JWT (an authenticated
/// <see cref="Microsoft.AspNetCore.Http.HttpContext.User"/>) OR an
/// <c>X-Service-Token</c> header matching the configured
/// <see cref="ServiceAuthOptions.Token"/> (service-to-service calls, e.g. the
/// Moderation service applying a Hide/Remove decision to a story or chapter).
/// Pair with <c>[AllowAnonymous]</c> so authentication still populates the
/// principal without a controller-level <c>[Authorize]</c> rejecting a
/// token-only caller.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ServiceOrUserAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// Allows the request through when the caller is either an authenticated
    /// user (valid JWT) or presents the correct <c>X-Service-Token</c> header;
    /// otherwise short-circuits the pipeline with 401.
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        // Already-authenticated user (valid JWT) is let through as-is.
        if (http.User?.Identity?.IsAuthenticated ?? false)
        {
            await next();
            return;
        }

        var configuredToken = http.RequestServices
            .GetRequiredService<IOptions<ServiceAuthOptions>>().Value.Token;

        // Otherwise fall back to the shared service token header, compared in
        // fixed time to avoid leaking its value via a timing side channel.
        if (!string.IsNullOrEmpty(configuredToken)
            && http.Request.Headers.TryGetValue(ServiceAuthConstants.HeaderName, out var provided)
            && FixedTimeEquals(provided.ToString(), configuredToken))
        {
            await next();
            return;
        }

        // Neither check passed: reject the request.
        context.Result = new UnauthorizedResult();
    }

    /// <summary>Constant-time string equality check, to prevent timing attacks on the service token.</summary>
    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
