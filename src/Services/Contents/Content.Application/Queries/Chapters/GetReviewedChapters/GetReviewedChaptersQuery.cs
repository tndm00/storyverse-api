namespace Content.Application.Queries.Chapters.GetReviewedChapters;

/// <summary>
/// Chapters a moderator has already decided on: <c>status=Approved</c> or
/// <c>status=Rejected</c> (from the review audit trail). Requires
/// <c>content.moderate</c>.
/// </summary>
public sealed class GetReviewedChaptersQuery : IQuery<PagedResponseDto<PendingReviewChapterResponseDto>>
{
    /// <summary>"Approved" or "Rejected"; anything else is treated as "Approved".</summary>
    public string Status { get; init; }

    /// <summary>Free-text match against the chapter title or its story title.</summary>
    public string Keyword { get; init; }

    /// <summary>Queue item type; any value other than "Chapter" yields an empty page.</summary>
    public string Type { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
