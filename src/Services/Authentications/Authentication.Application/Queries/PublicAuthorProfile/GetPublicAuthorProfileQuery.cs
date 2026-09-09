namespace Authentication.Application.Queries.PublicAuthorProfile;

/// <summary>
/// Public author profile by author-profile id. Anonymous — used by the reader
/// site's <c>/author/:id</c> page.
/// </summary>
public sealed class GetPublicAuthorProfileQuery : IQuery<PublicAuthorProfileResponseDto>
{
    public long AuthorProfileId { get; init; }
}
