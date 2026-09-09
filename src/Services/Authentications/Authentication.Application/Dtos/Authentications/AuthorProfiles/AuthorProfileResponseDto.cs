namespace Authentication.Application.Dtos.Authentications.AuthorProfiles;

/// <summary>
/// The caller's publishing identity. Returned by the create endpoint and by
/// <c>GET /v1/auth/author-profile</c>. Payout info is intentionally omitted
/// (Phase 2, and not needed by any Phase 1 caller).
/// </summary>
public sealed class AuthorProfileResponseDto
{
    public long AuthorProfileId { get; init; }

    public long UserId { get; init; }

    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }

    public string AvatarUrl { get; init; }

    public string BannerUrl { get; init; }

    public bool Verified { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// True when the access token that will authorize publishing calls is only
    /// issued on the caller's next login/refresh — the <c>author_id</c> claim is
    /// not retro-fitted into an already-issued token.
    /// </summary>
    public bool RequiresTokenRefresh { get; init; }
}
