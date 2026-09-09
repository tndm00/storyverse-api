namespace Content.Application.Commands.Chapters.ApproveChapter;

public sealed class ApproveChapterCommandValidator : AbstractValidator<ApproveChapterCommand>
{
    public ApproveChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
