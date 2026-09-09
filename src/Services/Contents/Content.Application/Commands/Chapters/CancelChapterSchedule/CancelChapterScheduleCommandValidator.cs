namespace Content.Application.Commands.Chapters.CancelChapterSchedule;

public sealed class CancelChapterScheduleCommandValidator : AbstractValidator<CancelChapterScheduleCommand>
{
    public CancelChapterScheduleCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
