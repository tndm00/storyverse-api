namespace Content.Application.Commands.Chapters.UpdateChapter;

public sealed class UpdateChapterCommandValidator : AbstractValidator<UpdateChapterCommand>
{
    public UpdateChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxChapterTitleLength);

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ApplicationErrorConstants.ContentRequired);

        RuleFor(x => x.OrderIndex).GreaterThan(0);
    }
}
