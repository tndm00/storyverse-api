namespace Community.Application.Commands.Ratings.UpsertRating;

/// <summary>Creates or replaces the caller's rating for a story (one per user).</summary>
public sealed class UpsertRatingCommand : ICommand<RatingResponseDto>
{
    public Guid StoryId { get; init; }

    public int Score { get; init; }

    public string ReviewText { get; init; }
}
