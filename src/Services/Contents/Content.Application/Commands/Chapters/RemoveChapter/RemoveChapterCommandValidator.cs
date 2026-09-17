namespace Content.Application.Commands.Chapters.RemoveChapter;

public sealed class RemoveChapterCommandValidator : AbstractValidator<RemoveChapterCommand>
{
    /// <summary>
    /// Requires a non-empty chapter id.
    /// </summary>
    public RemoveChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
