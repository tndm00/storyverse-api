namespace Community.Application.Queries.Ratings.GetStoryRatings;

/// <summary>Paged list of a story's ratings, newest first.</summary>
public sealed class GetStoryRatingsQuery : IQuery<PagedResponseDto<RatingResponseDto>>
{
    public Guid StoryId { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
