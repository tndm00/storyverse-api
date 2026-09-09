using Be.StoryVerse.ApiCommon.Authorization;
using Be.StoryVerse.Shared.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Be.StoryVerse.ApiCommon.Extensions;

/// <summary>
/// Registers StoryVerse permission-based authorization: the shared
/// <see cref="PermissionAuthorizationHandler"/> plus one policy per permission
/// in <see cref="StoryVersePermissions.All"/> (named
/// <c>perm:&lt;permission&gt;</c>, matching <see cref="HasPermissionAttribute"/>).
/// Call this in a service's API wiring instead of a bare
/// <c>services.AddAuthorization()</c>.
/// </summary>
public static class AuthorizationServiceExtensions
{
    public static IServiceCollection AddStoryVersePermissions(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            foreach (var permission in StoryVersePermissions.All)
            {
                options.AddPolicy(
                    HasPermissionAttribute.PolicyPrefix + permission,
                    policy => policy.Requirements.Add(new PermissionRequirement(permission)));
            }
        });

        return services;
    }
}
