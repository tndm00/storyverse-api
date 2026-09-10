namespace Authentication.Application.Dtos.Authentications.Admin;

/// <summary>
/// Body of <c>POST /v1/auth/admin/users/{userId}/roles</c>. <see cref="Role"/> is
/// one of the platform role names (<c>Reader</c>, <c>Author</c>, <c>Moderator</c>,
/// <c>PlatformAdmin</c>).
/// </summary>
public sealed class GrantUserRoleRequestDto
{
    public string Role { get; init; } = string.Empty;
}
