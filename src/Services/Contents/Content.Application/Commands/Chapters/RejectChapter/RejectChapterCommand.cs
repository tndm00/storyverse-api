namespace Content.Application.Commands.Chapters.RejectChapter;

/// <summary>
/// A moderator rejects a chapter in review: InReview -&gt; Rejected. The author
/// sees <see cref="Reason"/> and may edit and resubmit. Requires <c>content.moderate</c>.
/// </summary>
public sealed class RejectChapterCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }

    public string Reason { get; init; } = string.Empty;
}
