namespace Community.Application.Queries.Ratings.GetStoryRatings;

public sealed class GetStoryRatingsQueryHandler
    : IQueryHandler<GetStoryRatingsQuery, PagedResponseDto<RatingResponseDto>>
{
    private readonly IRatingRepository _ratingRepository;

    public GetStoryRatingsQueryHandler(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<PagedResponseDto<RatingResponseDto>> Handle(
        GetStoryRatingsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = PagingParameters.Normalize(request.PageNumber, request.PageSize);

        var (items, totalCount) = await _ratingRepository.GetByStoryAsync(
            request.StoryId, pageNumber, pageSize, cancellationToken);

        var dtos = items.Select(CommunityDtoMapper.ToDto).ToArray();

        return PagedResponseDto<RatingResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
