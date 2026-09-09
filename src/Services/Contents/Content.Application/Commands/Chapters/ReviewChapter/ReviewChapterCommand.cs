namespace Content.Application.Commands.Chapters.ReviewChapter;

/// <summary>
/// A moderator picks a pending chapter off the review queue: PendingReview -&gt;
/// InReview. Requires <c>content.moderate</c>.
/// </summary>
public sealed class ReviewChapterCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }
}
