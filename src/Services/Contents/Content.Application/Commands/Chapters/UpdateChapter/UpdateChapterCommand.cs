namespace Content.Application.Commands.Chapters.UpdateChapter;

/// <summary>
/// Updates a chapter's title/order/content. Owner only. Editing a chapter that is
/// already published is allowed but logged — substantial rewrites should use
/// versioning (not built in Phase 1A).
/// </summary>
public sealed class UpdateChapterCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }

    public string Title { get; init; } = string.Empty;

    public decimal OrderIndex { get; init; }

    public string Content { get; init; } = string.Empty;

    public Guid? VolumeId { get; init; }
}
