namespace Content.Application.Commands.Chapters.PublishChapter;

public sealed class PublishChapterCommandValidator : AbstractValidator<PublishChapterCommand>
{
    public PublishChapterCommandValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
