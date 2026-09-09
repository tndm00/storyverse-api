namespace Community.Application.Commands.Comments.AddComment;

public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ApplicationErrorConstants.CommentContentRequired)
            .MaximumLength(ApplicationConstants.MaxCommentLength);
    }
}
