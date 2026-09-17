namespace Content.Application.Commands.Chapters.CancelChapterSchedule;

public sealed class CancelChapterScheduleCommandValidator : AbstractValidator<CancelChapterScheduleCommand>
{
    /// <summary>
    /// Requires a non-empty chapter id.
    /// </summary>
    public CancelChapterScheduleCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
