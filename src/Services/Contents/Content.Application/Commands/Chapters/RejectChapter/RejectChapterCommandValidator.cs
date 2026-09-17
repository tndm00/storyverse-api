namespace Content.Application.Commands.Chapters.RejectChapter;

public sealed class RejectChapterCommandValidator : AbstractValidator<RejectChapterCommand>
{
    /// <summary>
    /// Requires a non-empty chapter id and a non-empty rejection reason within
    /// the max length.
    /// </summary>
    public RejectChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(ApplicationErrorConstants.RejectionReasonRequired)
            .MaximumLength(2000);
    }
}
