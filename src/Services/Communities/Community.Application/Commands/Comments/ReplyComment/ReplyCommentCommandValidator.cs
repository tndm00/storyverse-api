namespace Community.Application.Commands.Comments.ReplyComment;

/// <summary>Validation rules for <see cref="ReplyCommentCommand"/>.</summary>
public sealed class ReplyCommentCommandValidator : AbstractValidator<ReplyCommentCommand>
{
    /// <summary>Requires a parent comment id and non-empty content within the configured max length.</summary>
    public ReplyCommentCommandValidator()
    {
        RuleFor(x => x.ParentCommentId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ApplicationErrorConstants.CommentContentRequired)
            .MaximumLength(ApplicationConstants.MaxCommentLength);
    }
}
