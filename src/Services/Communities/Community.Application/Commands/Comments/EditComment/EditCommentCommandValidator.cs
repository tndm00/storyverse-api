namespace Community.Application.Commands.Comments.EditComment;

/// <summary>Validation rules for <see cref="EditCommentCommand"/>.</summary>
public sealed class EditCommentCommandValidator : AbstractValidator<EditCommentCommand>
{
    /// <summary>Requires a comment id and non-empty content within the configured max length.</summary>
    public EditCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ApplicationErrorConstants.CommentContentRequired)
            .MaximumLength(ApplicationConstants.MaxCommentLength);
    }
}
