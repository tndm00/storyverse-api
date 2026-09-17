namespace Content.Application.Queries.Chapters.GetChapterContent;

public sealed class GetChapterContentQueryValidator : AbstractValidator<GetChapterContentQuery>
{
    /// <summary>Requires a non-empty chapter id.</summary>
    public GetChapterContentQueryValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
