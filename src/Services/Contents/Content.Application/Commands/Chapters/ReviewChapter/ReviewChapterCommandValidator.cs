namespace Content.Application.Commands.Chapters.ReviewChapter;

public sealed class ReviewChapterCommandValidator : AbstractValidator<ReviewChapterCommand>
{
    public ReviewChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
