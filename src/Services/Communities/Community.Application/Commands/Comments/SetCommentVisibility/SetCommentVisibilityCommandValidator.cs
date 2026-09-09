namespace Community.Application.Commands.Comments.SetCommentVisibility;

public sealed class SetCommentVisibilityCommandValidator : AbstractValidator<SetCommentVisibilityCommand>
{
    public SetCommentVisibilityCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
    }
}
