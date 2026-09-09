using Microsoft.AspNetCore.Authorization;

namespace Be.StoryVerse.ApiCommon.Authorization;

/// <summary>
/// Authorization requirement satisfied when the caller holds a specific
/// business permission (see <c>Be.StoryVerse.Shared.Authorization.StoryVersePermissions</c>).
/// </summary>
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
