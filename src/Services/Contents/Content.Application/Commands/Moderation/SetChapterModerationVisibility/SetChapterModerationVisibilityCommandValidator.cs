namespace Content.Application.Commands.Moderation.SetChapterModerationVisibility;

public sealed class SetChapterModerationVisibilityCommandValidator
    : AbstractValidator<SetChapterModerationVisibilityCommand>
{
    /// <summary>Validates that a chapter id is provided.</summary>
    public SetChapterModerationVisibilityCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
