namespace Content.Application.Commands.Stories.DeleteStory;

public sealed class DeleteStoryCommandValidator : AbstractValidator<DeleteStoryCommand>
{
    /// <summary>Validates that a story id was provided.</summary>
    public DeleteStoryCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
