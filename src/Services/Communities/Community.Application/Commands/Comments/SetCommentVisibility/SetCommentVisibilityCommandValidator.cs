namespace Community.Application.Commands.Comments.SetCommentVisibility;

/// <summary>Validation rules for <see cref="SetCommentVisibilityCommand"/>.</summary>
public sealed class SetCommentVisibilityCommandValidator : AbstractValidator<SetCommentVisibilityCommand>
{
    /// <summary>Requires a non-empty comment id.</summary>
    public SetCommentVisibilityCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
    }
}
