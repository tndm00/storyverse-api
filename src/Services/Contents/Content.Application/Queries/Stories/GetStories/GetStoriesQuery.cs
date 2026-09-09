namespace Content.Application.Queries.Stories.GetStories;

/// <summary>Public discovery listing. Draft stories are never returned.</summary>
public sealed class GetStoriesQuery : IQuery<PagedResponseDto<StorySummaryResponseDto>>
{
    public string GenreSlug { get; init; }

    public string TagSlug { get; init; }

    public long? AuthorProfileId { get; init; }

    public string Status { get; init; }

    public string SortBy { get; init; }

    public string SortDirection { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
