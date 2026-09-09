namespace Community.Application.Queries.Ratings.GetMyRating;

public sealed class GetMyRatingQueryValidator : AbstractValidator<GetMyRatingQuery>
{
    public GetMyRatingQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
