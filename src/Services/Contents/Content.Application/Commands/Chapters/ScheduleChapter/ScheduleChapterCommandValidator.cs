namespace Content.Application.Commands.Chapters.ScheduleChapter;

public sealed class ScheduleChapterCommandValidator : AbstractValidator<ScheduleChapterCommand>
{
    public ScheduleChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(_ => DateTime.UtcNow)
            .WithMessage(ApplicationErrorConstants.ScheduledTimeMustBeFuture);
    }
}
