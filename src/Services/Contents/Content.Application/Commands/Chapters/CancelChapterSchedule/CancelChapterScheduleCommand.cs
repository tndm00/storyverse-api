namespace Content.Application.Commands.Chapters.CancelChapterSchedule;

/// <summary>Cancels a chapter's schedule, returning it to draft. Owner only.</summary>
public sealed class CancelChapterScheduleCommand : ICommand<ChapterDetailResponseDto>
{
    public Guid ChapterId { get; init; }
}
