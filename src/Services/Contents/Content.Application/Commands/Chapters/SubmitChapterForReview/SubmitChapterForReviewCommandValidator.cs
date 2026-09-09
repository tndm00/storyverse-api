namespace Content.Application.Commands.Chapters.SubmitChapterForReview;

public sealed class SubmitChapterForReviewCommandValidator : AbstractValidator<SubmitChapterForReviewCommand>
{
    public SubmitChapterForReviewCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
