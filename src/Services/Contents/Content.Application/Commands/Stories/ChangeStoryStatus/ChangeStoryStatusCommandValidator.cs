namespace Content.Application.Commands.Stories.ChangeStoryStatus;

public sealed class ChangeStoryStatusCommandValidator : AbstractValidator<ChangeStoryStatusCommand>
{
    public ChangeStoryStatusCommandValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();

        RuleFor(x => x.TargetStatus)
            .IsInEnum();
    }
}
