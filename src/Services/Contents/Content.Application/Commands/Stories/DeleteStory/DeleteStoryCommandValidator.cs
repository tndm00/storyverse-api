namespace Content.Application.Commands.Stories.DeleteStory;

public sealed class DeleteStoryCommandValidator : AbstractValidator<DeleteStoryCommand>
{
    public DeleteStoryCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
