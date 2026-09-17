namespace Content.Application.Commands.Chapters.ReviewChapter;

public sealed class ReviewChapterCommandValidator : AbstractValidator<ReviewChapterCommand>
{
    /// <summary>
    /// Requires a non-empty chapter id.
    /// </summary>
    public ReviewChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
