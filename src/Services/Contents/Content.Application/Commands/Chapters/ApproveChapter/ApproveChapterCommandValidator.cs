namespace Content.Application.Commands.Chapters.ApproveChapter;

public sealed class ApproveChapterCommandValidator : AbstractValidator<ApproveChapterCommand>
{
    /// <summary>
    /// Requires a non-empty chapter id.
    /// </summary>
    public ApproveChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
