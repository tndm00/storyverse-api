namespace Content.Application.Commands.Moderation.SetStoryModerationVisibility;

public sealed class SetStoryModerationVisibilityCommandValidator
    : AbstractValidator<SetStoryModerationVisibilityCommand>
{
    /// <summary>Validates that a story id is provided.</summary>
    public SetStoryModerationVisibilityCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
