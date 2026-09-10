namespace Content.Application.Queries.Chapters.GetPendingReviewChapters;

/// <summary>
/// Cross-story moderation queue. Requires <c>content.moderate</c>.
/// </summary>
public sealed class GetPendingReviewChaptersQuery : IQuery<PagedResponseDto<PendingReviewChapterResponseDto>>
{
    /// <summary>"PendingReview" or "InReview"; null returns both.</summary>
    public string Status { get; init; }

    /// <summary>Free-text match against the chapter title or its story title.</summary>
    public string Keyword { get; init; }

    /// <summary>
    /// Optional queue item type. The queue is chapter-only, so any value other
    /// than "Chapter" yields an empty page instead of an error.
    /// </summary>
    public string Type { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
