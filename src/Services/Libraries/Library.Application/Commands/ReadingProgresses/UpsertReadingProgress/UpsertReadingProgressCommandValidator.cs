namespace Library.Application.Commands.ReadingProgresses.UpsertReadingProgress;

public sealed class UpsertReadingProgressCommandValidator : AbstractValidator<UpsertReadingProgressCommand>
{
    public UpsertReadingProgressCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);

        RuleFor(x => x.LastChapterId).NotEmpty().WithMessage(ApplicationErrorConstants.ChapterIdRequired);

        RuleFor(x => x.ScrollPercent)
            .InclusiveBetween(ApplicationConstants.MinScrollPercent, ApplicationConstants.MaxScrollPercent)
            .When(x => x.ScrollPercent.HasValue)
            .WithMessage(ApplicationErrorConstants.InvalidScrollPercent);
    }
}
