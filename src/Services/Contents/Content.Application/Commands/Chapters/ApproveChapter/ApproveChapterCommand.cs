namespace Content.Application.Commands.Chapters.ApproveChapter;

/// <summary>
/// A moderator approves a chapter in review: InReview -&gt; Published. On the
/// story's first approved chapter the story flips Draft -&gt; Ongoing, exactly
/// as it did when authors could publish directly. Requires <c>content.moderate</c>.
/// </summary>
public sealed class ApproveChapterCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }
}
