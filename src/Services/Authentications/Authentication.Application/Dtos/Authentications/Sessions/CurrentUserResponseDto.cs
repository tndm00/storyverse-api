namespace Authentication.Application.Dtos.Authentications.Sessions;

/// <summary>
/// Basic profile info for <c>GET /v1/auth/me</c>, resolved from the trusted
/// auth context per auth-guidelines.md section 3 (Identity Source Rules).
/// </summary>
public sealed record CurrentUserResponseDto
{
    public long UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string AvatarUrl { get; init; }

    public DateTimeOffset? LastLoginAt { get; init; }

    /// <summary>
    /// The caller's role names (e.g. <c>Reader</c>, <c>Author</c>). Frontends use
    /// this for menu gating only — server-side authorization is authoritative.
    /// </summary>
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}
