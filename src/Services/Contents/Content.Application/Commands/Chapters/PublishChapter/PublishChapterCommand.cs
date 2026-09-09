namespace Content.Application.Commands.Chapters.PublishChapter;

/// <summary>
/// Publishes a draft or scheduled chapter. Owner only. Requires the story to have
/// exactly one primary genre; transitions a still-draft story to Ongoing.
/// </summary>
public sealed class PublishChapterCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }
}
