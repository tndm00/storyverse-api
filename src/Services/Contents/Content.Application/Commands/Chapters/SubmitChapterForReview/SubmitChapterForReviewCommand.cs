namespace Content.Application.Commands.Chapters.SubmitChapterForReview;

/// <summary>
/// Author submits a draft (or resubmits a rejected) chapter for moderation.
/// Owner only. Requires the story to have exactly one primary genre. Does not
/// publish the chapter — a moderator must approve it first (see
/// <c>ApproveChapterCommand</c>).
/// </summary>
public sealed class SubmitChapterForReviewCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }
}
