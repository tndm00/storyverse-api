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
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var http = context.HttpContext;

        var userAuthenticated = http.User?.Identity?.IsAuthenticated ?? false;
        if (userAuthenticated)
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
