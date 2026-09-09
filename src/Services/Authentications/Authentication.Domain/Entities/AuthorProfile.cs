namespace Authentication.Domain.Entities;

/// <summary>
/// Publishing identity for a <see cref="User"/> who writes/publishes stories.
/// A user may or may not have an AuthorProfile (0..1 relationship).
/// </summary>
public sealed class AuthorProfile : BaseEntity
{
    public long UserId { get; set; }

    public string PenName { get; set; } = string.Empty;

    public string Bio { get; set; }

    public string AvatarUrl { get; set; }

    public string BannerUrl { get; set; }

    public bool Verified { get; set; }

    public AuthorProfileStatus Status { get; set; } = AuthorProfileStatus.Active;

    /// <summary>
    /// Payout information (Phase 2). Stored as JSON, unused until payouts ship.
    /// </summary>
    public string PayoutInfo { get; set; }
}
