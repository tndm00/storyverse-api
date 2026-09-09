using Microsoft.AspNetCore.Authorization;

namespace Be.StoryVerse.ApiCommon.Authorization;

/// <summary>
/// Guards a controller or action with a business permission, per
/// auth-guidelines.md section 4 (policy-based access control, no hardcoded role
/// checks in controllers).
/// <para>Usage: <c>[HasPermission(StoryVersePermissions.Genres.Manage)]</c>.</para>
/// </summary>
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "perm:";

    public HasPermissionAttribute(string permission)
        : base(PolicyPrefix + permission)
    {
    }
}
