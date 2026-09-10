namespace Content.Application.Commands.Moderation.SetStoryModerationVisibility;

public sealed class SetStoryModerationVisibilityCommandValidator
    : AbstractValidator<SetStoryModerationVisibilityCommand>
{
    public SetStoryModerationVisibilityCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
