namespace Community.Application.Commands.Comments.AddComment;

/// <summary>Validation rules for <see cref="AddCommentCommand"/>.</summary>
public sealed class AddCommentCommandValidator : AbstractValidator<AddCommentCommand>
{
    /// <summary>Requires a chapter id and non-empty content within the configured max length.</summary>
    public AddCommentCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(ApplicationErrorConstants.CommentContentRequired)
            .MaximumLength(ApplicationConstants.MaxCommentLength);
    }
}
