namespace Authentication.Application.Queries.AuthorProfileLookup;

/// <summary>
/// Internal lookup: the owning user id for an AuthorProfile id. Reachable only
/// through the service-token-protected internal endpoint.
/// </summary>
public sealed class GetAuthorProfileLookupQuery : IQuery<AuthorProfileLookupResponseDto>
{
    public long AuthorProfileId { get; init; }
}
