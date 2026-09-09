namespace Community.Application.Queries.Comments.GetChapterComments;

public sealed class GetChapterCommentsQueryValidator : AbstractValidator<GetChapterCommentsQuery>
{
    public GetChapterCommentsQueryValidator()
    {
        RuleFor(x => x.ChapterId).NotEmpty();
    }
}
