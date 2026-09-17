namespace Content.Application.Commands.Chapters.CreateChapter;

public sealed class CreateChapterCommandValidator : AbstractValidator<CreateChapterCommand>
{
    /// <summary>
    /// Requires a story id, a non-empty title within the max length, non-empty
    /// content, and a non-negative order index.
    /// </summary>
    public CreateChapterCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxChapterTitleLength);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ApplicationErrorConstants.ContentRequired);

        RuleFor(x => x.OrderIndex).GreaterThanOrEqualTo(0);
    }
}
