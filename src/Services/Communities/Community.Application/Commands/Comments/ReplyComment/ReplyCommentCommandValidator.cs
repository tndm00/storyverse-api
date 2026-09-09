namespace Community.Application.Commands.Comments.ReplyComment;

public sealed class ReplyCommentCommandValidator : AbstractValidator<ReplyCommentCommand>
{
    public ReplyCommentCommandValidator()
    {
        RuleFor(x => x.ParentCommentId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ApplicationErrorConstants.CommentContentRequired)
            .MaximumLength(ApplicationConstants.MaxCommentLength);
    }
}
