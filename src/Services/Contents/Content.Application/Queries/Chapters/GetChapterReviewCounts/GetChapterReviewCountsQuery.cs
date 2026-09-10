namespace Content.Application.Queries.Chapters.GetChapterReviewCounts;

/// <summary>
/// Moderation review dashboard counters (Pending, InReview, Approved, Rejected).
/// Requires <c>content.moderate</c>.
/// </summary>
public sealed class GetChapterReviewCountsQuery : IQuery<ChapterReviewCountsResponseDto>
{
}
