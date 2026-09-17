namespace Community.Application.Commands.Comments.DeleteComment;

/// <summary>Validation rules for <see cref="DeleteCommentCommand"/>.</summary>
public sealed class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    /// <summary>Requires a non-empty comment id.</summary>
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
    }
}
