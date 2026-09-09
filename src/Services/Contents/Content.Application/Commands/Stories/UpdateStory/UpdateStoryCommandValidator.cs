namespace Content.Application.Commands.Stories.UpdateStory;

public sealed class UpdateStoryCommandValidator : AbstractValidator<UpdateStoryCommand>
{
    public UpdateStoryCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(ApplicationErrorConstants.TitleRequired)
            .MaximumLength(ApplicationConstants.MaxTitleLength);

        RuleFor(x => x.Description)
            .MaximumLength(ApplicationConstants.MaxDescriptionLength);

        RuleFor(x => x.CoverImageUrl)
            .MaximumLength(ApplicationConstants.MaxUrlLength);

        RuleFor(x => x.Language)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.OriginalSource)
            .NotEmpty()
            .When(x => x.ContentType == StoryContentType.Translated)
            .WithMessage(ApplicationErrorConstants.OriginalSourceRequired);
    }
}
