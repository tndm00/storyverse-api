namespace Community.Application.Queries.Ratings.GetStoryRatings;

/// <summary>Handles <see cref="GetStoryRatingsQuery"/>: fetches a page of a story's ratings, newest first.</summary>
public sealed class GetStoryRatingsQueryHandler
    : IQueryHandler<GetStoryRatingsQuery, PagedResponseDto<RatingResponseDto>>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IUserDirectoryClient _userDirectory;

    /// <summary>Creates the handler with its repository and user-directory dependencies.</summary>
    public GetStoryRatingsQueryHandler(
        IRatingRepository ratingRepository,
        IUserDirectoryClient userDirectory)
    {
        _ratingRepository = ratingRepository;
        _userDirectory = userDirectory;
    }

    /// <summary>Returns a normalized, paged list of ratings for the story with author display names attached.</summary>
    public async Task<PagedResponseDto<RatingResponseDto>> Handle(
        GetStoryRatingsQuery request,
        CancellationToken cancellationToken)
    {
        // Clamp the requested page number/size to the allowed range.
        var (pageNumber, pageSize) = PagingParameters.Normalize(request.PageNumber, request.PageSize);

        // Fetch the requested page of ratings along with the total count for paging metadata.
        var (items, totalCount) = await _ratingRepository.GetByStoryAsync(
            request.StoryId, pageNumber, pageSize, cancellationToken);

        // Resolve display names for all raters in the page in a single batched call.
        var names = await _userDirectory.GetDisplayNamesAsync(
            items.Select(r => r.UserId), cancellationToken);

        // Map each rating entity to its response DTO, attaching the resolved display name if any.
        var dtos = items
            .Select(r => CommunityDtoMapper.ToDto(
                r, names.TryGetValue(r.UserId, out var name) ? name : null))
            .ToArray();

        return PagedResponseDto<RatingResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
