namespace Content.Application.Queries.Chapters.GetChapterContent;

public sealed class GetChapterContentQueryValidator : AbstractValidator<GetChapterContentQuery>
{
    public GetChapterContentQueryValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
