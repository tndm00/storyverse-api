namespace Content.Application.Commands.Chapters.ScheduleChapter;

public sealed class ScheduleChapterCommandValidator : AbstractValidator<ScheduleChapterCommand>
{
    /// <summary>
    /// Requires a non-empty chapter id and a scheduled time strictly in the future.
    /// </summary>
    public ScheduleChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(_ => DateTime.UtcNow)
            .WithMessage(ApplicationErrorConstants.ScheduledTimeMustBeFuture);
    }
}
