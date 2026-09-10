namespace Community.Application.Queries.Ratings.GetMyRating;

public sealed class GetMyRatingQueryHandler : IQueryHandler<GetMyRatingQuery, RatingResponseDto>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly IUserDirectoryClient _userDirectory;

    public GetMyRatingQueryHandler(
        IRatingRepository ratingRepository,
        ICurrentUserContext userContext,
        IUserDirectoryClient userDirectory)
    {
        _ratingRepository = ratingRepository;
        _userContext = userContext;
        _userDirectory = userDirectory;
    }

    public async Task<RatingResponseDto> Handle(GetMyRatingQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        var rating = await _ratingRepository.GetByStoryAndUserAsync(request.StoryId, userId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.RatingNotFound);

        var names = await _userDirectory.GetDisplayNamesAsync(new[] { rating.UserId }, cancellationToken);

        return CommunityDtoMapper.ToDto(rating, names.TryGetValue(rating.UserId, out var name) ? name : null);
    }
}
