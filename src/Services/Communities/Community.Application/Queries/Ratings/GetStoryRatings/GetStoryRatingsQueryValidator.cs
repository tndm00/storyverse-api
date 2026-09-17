namespace Community.Application.Queries.Ratings.GetStoryRatings;

/// <summary>Validation rules for <see cref="GetStoryRatingsQuery"/>.</summary>
public sealed class GetStoryRatingsQueryValidator : AbstractValidator<GetStoryRatingsQuery>
{
    /// <summary>Requires a non-empty story id.</summary>
    public GetStoryRatingsQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
