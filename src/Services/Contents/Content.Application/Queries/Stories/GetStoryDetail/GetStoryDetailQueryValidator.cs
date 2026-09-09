namespace Content.Application.Queries.Stories.GetStoryDetail;

public sealed class GetStoryDetailQueryValidator : AbstractValidator<GetStoryDetailQuery>
{
    public GetStoryDetailQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
    }
}
