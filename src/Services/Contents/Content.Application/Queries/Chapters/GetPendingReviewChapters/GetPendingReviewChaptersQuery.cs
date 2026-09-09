namespace Content.Application.Queries.Chapters.GetPendingReviewChapters;

/// <summary>
/// Cross-story moderation queue. Requires <c>content.moderate</c>.
/// </summary>
public sealed class GetPendingReviewChaptersQuery : IQuery<PagedResponseDto<PendingReviewChapterResponseDto>>
{
    /// <summary>"PendingReview" or "InReview"; null returns both.</summary>
    public string Status { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
