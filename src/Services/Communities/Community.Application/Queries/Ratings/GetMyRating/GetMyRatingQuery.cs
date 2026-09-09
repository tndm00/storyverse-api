namespace Community.Application.Queries.Ratings.GetMyRating;

/// <summary>Returns the caller's own rating for a story, or 404 if none exists.</summary>
public sealed class GetMyRatingQuery : IQuery<RatingResponseDto>
{
    public Guid StoryId { get; init; }
}
