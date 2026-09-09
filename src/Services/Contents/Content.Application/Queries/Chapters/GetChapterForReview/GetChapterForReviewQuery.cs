namespace Content.Application.Queries.Chapters.GetChapterForReview;

/// <summary>
/// Full chapter detail for a moderator, regardless of status or ownership.
/// Requires <c>content.moderate</c>.
/// </summary>
public sealed class GetChapterForReviewQuery : IQuery<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }
}
