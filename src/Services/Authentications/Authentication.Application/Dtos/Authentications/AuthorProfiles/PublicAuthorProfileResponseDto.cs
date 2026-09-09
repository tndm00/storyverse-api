namespace Authentication.Application.Dtos.Authentications.AuthorProfiles;

/// <summary>
/// The public view of an author, shown on the reader-site profile page
/// (<c>/author/:id</c>). No user id, no email, no payout info.
/// </summary>
public sealed class PublicAuthorProfileResponseDto
{
    public long AuthorProfileId { get; init; }

    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }

    public string AvatarUrl { get; init; }

    public string BannerUrl { get; init; }

    public bool Verified { get; init; }
}
