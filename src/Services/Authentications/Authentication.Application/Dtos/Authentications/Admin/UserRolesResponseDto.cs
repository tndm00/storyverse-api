namespace Authentication.Application.Dtos.Authentications.Admin;

/// <summary>
/// A user's current role grants after an admin change. The role name strings are
/// the stable JWT contract (<c>Be.StoryVerse.Shared.Authorization.StoryVerseRoles</c>).
/// The affected account only sees the new grant reflected in its token on the
/// next sign-in (<see cref="RequiresTokenRefresh"/>).
/// </summary>
public sealed class UserRolesResponseDto
{
    public long UserId { get; init; }

    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();

    public bool RequiresTokenRefresh { get; init; } = true;
}
