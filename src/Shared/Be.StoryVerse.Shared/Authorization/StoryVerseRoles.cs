namespace Be.StoryVerse.Shared.Authorization;

/// <summary>
/// Platform role names. Roles are permission groups (auth-guidelines.md section 6);
/// a role on its own is never checked in business logic — it is expanded to
/// permissions through <see cref="RolePermissionMap"/>.
/// <para>
/// These strings are the stable contract carried in the JWT <c>roles</c> claim
/// and must match the Authentication service's <c>Role</c> enum names exactly.
/// </para>
/// </summary>
public static class StoryVerseRoles
{
    public const string Reader = "Reader";
    public const string Author = "Author";
    public const string Moderator = "Moderator";
    public const string PlatformAdmin = "PlatformAdmin";
}
