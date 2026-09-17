using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Notification.Application.Options;

namespace Notification.Api.Security;

/// <summary>
/// Minimal gate for the internal notification-creation endpoint: the request is
/// allowed when it carries EITHER a valid JWT (an authenticated
/// <see cref="Microsoft.AspNetCore.Http.HttpContext.User"/>, preserving the
/// existing FE reply-shim and e2e-script callers) OR an <c>X-Service-Token</c>
/// header matching the configured <see cref="ServiceAuthOptions.Token"/>
/// (service-to-service calls from the Content service). The action must also be
/// marked <c>[AllowAnonymous]</c> so authentication still populates the user
/// principal without the controller-level <c>[Authorize]</c> rejecting a
/// token-only caller first.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ServiceOrUserAuthorizeAttribute : Attribute, IAsyncActionFilter
{
    /// <summary>
    /// Allows the action to run when the caller is either an authenticated user
    /// or presents the correct <c>X-Service-Token</c> header; otherwise returns 401.
    /// </summary>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        // A valid JWT on the request always wins (normal end-user or FE reply-shim callers).
        var userAuthenticated = http.User?.Identity?.IsAuthenticated ?? false;
        if (userAuthenticated)
        {
            await next();
            return;
        }

        var configuredToken = http.RequestServices
            .GetRequiredService<IOptions<ServiceAuthOptions>>().Value.Token;

        // Otherwise fall back to the shared service-to-service token, compared in fixed time.
        if (!string.IsNullOrEmpty(configuredToken)
            && http.Request.Headers.TryGetValue(ServiceAuthConstants.HeaderName, out var provided)
            && FixedTimeEquals(provided.ToString(), configuredToken))
        {
            await next();
            return;
        }

        context.Result = new UnauthorizedResult();
    }

    /// <summary>Constant-time string comparison so token checks don't leak timing information.</summary>
    private static bool FixedTimeEquals(string a, string b)
    {
        var ba = Encoding.UTF8.GetBytes(a);
        var bb = Encoding.UTF8.GetBytes(b);
        return ba.Length == bb.Length && CryptographicOperations.FixedTimeEquals(ba, bb);
    }
}
