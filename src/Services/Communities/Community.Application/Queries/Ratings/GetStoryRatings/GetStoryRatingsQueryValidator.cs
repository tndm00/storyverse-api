namespace Community.Application.Queries.Ratings.GetStoryRatings;

public sealed class GetStoryRatingsQueryValidator : AbstractValidator<GetStoryRatingsQuery>
{
    public GetStoryRatingsQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
