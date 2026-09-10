namespace Content.Application.Dtos;

/// <summary>
/// Normalized inputs for the public story discovery listing. Built by
/// <c>GetStoriesQueryHandler</c> after validation and clamping.
/// </summary>
public sealed record StorySearchCriteria
{
    public string GenreSlug { get; init; }

    public string TagSlug { get; init; }

    /// <summary>Free-text match against the story title (admin/author listings only).</summary>
    public string Keyword { get; init; }

    /// <summary>Filter to one author's published stories (public author profile page).</summary>
    public long? AuthorProfileId { get; init; }

    public StoryStatus? Status { get; init; }

    public StorySortField SortBy { get; init; } = StorySortField.PublishedAt;

    public bool Descending { get; init; } = true;

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}

public enum StorySortField
{
    PublishedAt,
    Title,
    ViewCount,
    RatingAvg,
    CreatedAt
}
