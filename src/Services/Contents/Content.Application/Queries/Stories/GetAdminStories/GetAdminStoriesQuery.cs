namespace Content.Application.Queries.Stories.GetAdminStories;

/// <summary>
/// Admin catalog listing: stories in every status (Draft included), with
/// optional status/genre/keyword filters. Requires <c>content.moderate</c>.
/// </summary>
public sealed class GetAdminStoriesQuery : IQuery<PagedResponseDto<StorySummaryResponseDto>>
{
    public string Status { get; init; }

    public string GenreSlug { get; init; }

    /// <summary>Free-text match against the story title.</summary>
    public string Keyword { get; init; }

    public long? AuthorProfileId { get; init; }

    public string SortBy { get; init; }

    public string SortDirection { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
