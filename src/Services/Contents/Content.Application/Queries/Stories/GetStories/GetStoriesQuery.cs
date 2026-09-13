namespace Content.Application.Queries.Stories.GetStories;

/// <summary>Public discovery listing. Draft stories are never returned.</summary>
public sealed class GetStoriesQuery : IQuery<PagedResponseDto<StorySummaryResponseDto>>
{
    public string GenreSlug { get; init; }

    public string TagSlug { get; init; }

    /// <summary>Free-text match against the story title.</summary>
    public string Keyword { get; init; }

    public long? AuthorProfileId { get; init; }

    public string Status { get; init; }

    /// <summary>"long" (2+ published chapters) or "short" (exactly 1); any other value means no filter.</summary>
    public string Length { get; init; }

    public string SortBy { get; init; }

    public string SortDirection { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
