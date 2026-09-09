namespace Content.Application.Commands.Chapters.RejectChapter;

public sealed class RejectChapterCommandValidator : AbstractValidator<RejectChapterCommand>
{
    public RejectChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage(ApplicationErrorConstants.RejectionReasonRequired)
            .MaximumLength(2000);
    }
}
