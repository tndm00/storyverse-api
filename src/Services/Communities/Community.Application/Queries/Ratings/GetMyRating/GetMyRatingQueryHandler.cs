namespace Community.Application.Queries.Ratings.GetMyRating;

public sealed class GetMyRatingQueryHandler : IQueryHandler<GetMyRatingQuery, RatingResponseDto>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICurrentUserContext _userContext;

    public GetMyRatingQueryHandler(IRatingRepository ratingRepository, ICurrentUserContext userContext)
    {
        _ratingRepository = ratingRepository;
        _userContext = userContext;
    }

    public async Task<RatingResponseDto> Handle(GetMyRatingQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var rating = await _ratingRepository.GetByStoryAndUserAsync(request.StoryId, userId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.RatingNotFound);

        return CommunityDtoMapper.ToDto(rating);
    }
}
