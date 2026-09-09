using System.Linq;
using System.Threading.Tasks;
using Be.StoryVerse.Shared.Authorization;
using Be.StoryVerse.Shared.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Be.StoryVerse.ApiCommon.Authorization;

/// <summary>
/// Grants a <see cref="PermissionRequirement"/> when the caller's <c>roles</c>
/// claims expand — via <see cref="RolePermissionMap"/> — to include the
/// required permission. Roles are the only authorization data carried in the
/// token; the mapping to permissions is applied here, in every service.
/// </summary>
public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var roles = context.User
            .FindAll(AuthConstants.RolesClaimType)
            .Select(claim => claim.Value);

        if (RolePermissionMap.PermissionsFor(roles).Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
