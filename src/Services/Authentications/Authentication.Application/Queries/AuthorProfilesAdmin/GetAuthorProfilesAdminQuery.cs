namespace Authentication.Application.Queries.AuthorProfilesAdmin;

/// <summary>
/// Platform-admin author roster: every author profile (Active and Suspended),
/// optionally filtered by pen name keyword. Requires <c>users.manage</c>.
/// </summary>
public sealed class GetAuthorProfilesAdminQuery : IQuery<PagedResponseDto<AdminAuthorProfileResponseDto>>
{
    /// <summary>Free-text match against the pen name.</summary>
    public string Keyword { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
