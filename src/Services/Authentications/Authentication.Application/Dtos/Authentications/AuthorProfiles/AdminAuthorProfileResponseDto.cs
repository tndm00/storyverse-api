namespace Authentication.Application.Dtos.Authentications.AuthorProfiles;

/// <summary>
/// Author profile as seen by platform admins: adds the owning account's email
/// and display name (joined in from <c>Users</c>) on top of the fields a
/// caller sees on their own profile. Payout info is intentionally omitted.
/// </summary>
public sealed class AdminAuthorProfileResponseDto
{
    public long AuthorProfileId { get; init; }

    public long UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }

    public string AvatarUrl { get; init; }

    public string BannerUrl { get; init; }

    public bool Verified { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; }
}
