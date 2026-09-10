namespace Content.Application.Commands.Moderation.SetChapterModerationVisibility;

public sealed class SetChapterModerationVisibilityCommandValidator
    : AbstractValidator<SetChapterModerationVisibilityCommand>
{
    public SetChapterModerationVisibilityCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
