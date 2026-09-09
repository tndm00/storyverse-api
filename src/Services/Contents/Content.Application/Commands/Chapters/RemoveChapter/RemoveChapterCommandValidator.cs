namespace Content.Application.Commands.Chapters.RemoveChapter;

public sealed class RemoveChapterCommandValidator : AbstractValidator<RemoveChapterCommand>
{
    public RemoveChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
