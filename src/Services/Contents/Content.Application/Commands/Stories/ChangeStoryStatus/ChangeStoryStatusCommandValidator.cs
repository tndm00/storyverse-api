namespace Content.Application.Commands.Stories.ChangeStoryStatus;

public sealed class ChangeStoryStatusCommandValidator : AbstractValidator<ChangeStoryStatusCommand>
{
    /// <summary>Validates the story id and ensures the requested target status is a defined enum value.</summary>
    public ChangeStoryStatusCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.TargetStatus)
            .IsInEnum();
    }
}
