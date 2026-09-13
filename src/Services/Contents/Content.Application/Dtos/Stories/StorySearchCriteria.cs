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

    /// <summary>Filter by published-chapter count: Long = 2+ chapters, Short = exactly 1.</summary>
    public ChapterLengthFilter? ChapterLength { get; init; }

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
    CreatedAt,

    /// <summary>Sum of <see cref="Content.Domain.Entities.Chapter.CommentCount"/> over the story's chapters, computed on the fly.</summary>
    CommentCount
}

public enum ChapterLengthFilter
{
    Long,
    Short
}
