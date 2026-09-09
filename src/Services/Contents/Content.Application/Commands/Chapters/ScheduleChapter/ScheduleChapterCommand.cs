namespace Content.Application.Commands.Chapters.ScheduleChapter;

/// <summary>Schedules a draft chapter to publish automatically at a future time. Owner only.</summary>
public sealed class ScheduleChapterCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }

    public DateTime ScheduledAt { get; init; }
}
