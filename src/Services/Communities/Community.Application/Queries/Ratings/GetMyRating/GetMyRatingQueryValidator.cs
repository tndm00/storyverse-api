namespace Community.Application.Queries.Ratings.GetMyRating;

/// <summary>Validation rules for <see cref="GetMyRatingQuery"/>.</summary>
public sealed class GetMyRatingQueryValidator : AbstractValidator<GetMyRatingQuery>
{
    /// <summary>Requires a non-empty story id.</summary>
    public GetMyRatingQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
