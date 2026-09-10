namespace Community.Application.Queries.Ratings.GetStoryRatings;

public sealed class GetStoryRatingsQueryHandler
    : IQueryHandler<GetStoryRatingsQuery, PagedResponseDto<RatingResponseDto>>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IUserDirectoryClient _userDirectory;

    public GetStoryRatingsQueryHandler(
        IRatingRepository ratingRepository,
        IUserDirectoryClient userDirectory)
    {
        _ratingRepository = ratingRepository;
        _userDirectory = userDirectory;
    }

    public async Task<PagedResponseDto<RatingResponseDto>> Handle(
        GetStoryRatingsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PagingParameters.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _ratingRepository.GetByStoryAsync(
            request.StoryId, pageNumber, pageSize, cancellationToken);

        var names = await _userDirectory.GetDisplayNamesAsync(
            items.Select(r => r.UserId), cancellationToken);

        var dtos = items
            .Select(r => CommunityDtoMapper.ToDto(
                r, names.TryGetValue(r.UserId, out var name) ? name : null))
            .ToArray();

        return PagedResponseDto<RatingResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
