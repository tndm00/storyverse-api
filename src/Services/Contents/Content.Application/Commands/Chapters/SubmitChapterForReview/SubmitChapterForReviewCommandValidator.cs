namespace Content.Application.Commands.Chapters.SubmitChapterForReview;

public sealed class SubmitChapterForReviewCommandValidator : AbstractValidator<SubmitChapterForReviewCommand>
{
    /// <summary>
    /// Requires a non-empty chapter id.
    /// </summary>
    public SubmitChapterForReviewCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
