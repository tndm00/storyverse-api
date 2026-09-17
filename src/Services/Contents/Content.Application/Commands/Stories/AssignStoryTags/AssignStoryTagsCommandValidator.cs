namespace Content.Application.Commands.Stories.AssignStoryTags;

public sealed class AssignStoryTagsCommandValidator : AbstractValidator<AssignStoryTagsCommand>
{
    /// <summary>Validates the story id and enforces the max tag count and per-tag length limit.</summary>
    public AssignStoryTagsCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.Tags)
            .Must(tags => tags.Count <= ApplicationConstants.MaxTagsPerStory)
            .WithMessage(ApplicationErrorConstants.TooManyTags);

        RuleForEach(x => x.Tags)
            .MaximumLength(ApplicationConstants.MaxTagNameLength);
    }
}
