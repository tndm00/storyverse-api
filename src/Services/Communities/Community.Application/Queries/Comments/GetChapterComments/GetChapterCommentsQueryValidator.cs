namespace Community.Application.Queries.Comments.GetChapterComments;

/// <summary>Validates <see cref="GetChapterCommentsQuery"/>.</summary>
public sealed class GetChapterCommentsQueryValidator : AbstractValidator<GetChapterCommentsQuery>
{
    /// <summary>Requires a non-empty chapter id.</summary>
    public GetChapterCommentsQueryValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
