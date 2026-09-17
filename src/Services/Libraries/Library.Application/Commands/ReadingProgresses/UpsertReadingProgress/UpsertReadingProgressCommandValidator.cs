namespace Library.Application.Commands.ReadingProgresses.UpsertReadingProgress;

/// <summary>Validates <see cref="UpsertReadingProgressCommand"/> input before it reaches the handler.</summary>
public sealed class UpsertReadingProgressCommandValidator : AbstractValidator<UpsertReadingProgressCommand>
{
    public UpsertReadingProgressCommandValidator()
    {
        // Story and chapter must be specified.
        RuleFor(x => x.StoryId).NotEmpty().WithMessage(ApplicationErrorConstants.StoryIdRequired);

        RuleFor(x => x.LastChapterId).NotEmpty().WithMessage(ApplicationErrorConstants.ChapterIdRequired);

        // Scroll percent, when provided, must fall within the allowed range.
        RuleFor(x => x.ScrollPercent)
            .InclusiveBetween(ApplicationConstants.MinScrollPercent, ApplicationConstants.MaxScrollPercent)
            .When(x => x.ScrollPercent.HasValue)
            .WithMessage(ApplicationErrorConstants.InvalidScrollPercent);
    }
}
