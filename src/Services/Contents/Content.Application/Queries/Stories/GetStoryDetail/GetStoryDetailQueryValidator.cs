namespace Content.Application.Queries.Stories.GetStoryDetail;

public sealed class GetStoryDetailQueryValidator : AbstractValidator<GetStoryDetailQuery>
{
    /// <summary>Requires a non-empty slug.</summary>
    public GetStoryDetailQueryValidator()
    {
        RuleFor(x => x.Slug).NotEmpty();
    }
}
