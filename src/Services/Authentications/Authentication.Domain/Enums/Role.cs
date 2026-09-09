namespace Authentication.Domain.Enums;

/// <summary>
/// Platform roles (permission groups, auth-guidelines.md section 6). Enum names
/// are the contract carried in the JWT <c>roles</c> claim and must match
/// <c>Be.StoryVerse.Shared.Authorization.StoryVerseRoles</c> exactly.
/// </summary>
public enum Role
{
    Reader,
    Author,
    Moderator,
    PlatformAdmin
}
