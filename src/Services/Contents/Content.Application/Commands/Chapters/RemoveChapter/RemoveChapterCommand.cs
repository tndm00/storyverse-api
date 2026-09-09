namespace Content.Application.Commands.Chapters.RemoveChapter;

/// <summary>
/// Removes a published chapter from public view. Owner only in Phase 1A; the
/// moderator takedown path belongs to the Moderation service.
/// </summary>
public sealed class RemoveChapterCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }
}
