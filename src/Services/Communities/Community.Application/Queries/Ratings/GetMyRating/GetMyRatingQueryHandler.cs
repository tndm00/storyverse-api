namespace Community.Application.Queries.Ratings.GetMyRating;

/// <summary>Handles <see cref="GetMyRatingQuery"/>: fetches the caller's own rating for a story.</summary>
public sealed class GetMyRatingQueryHandler : IQueryHandler<GetMyRatingQuery, RatingResponseDto>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICurrentUserContext _userContext;
    private readonly IUserDirectoryClient _userDirectory;

    /// <summary>Creates the handler with its repository, user context and user-directory dependencies.</summary>
    public GetMyRatingQueryHandler(
        IRatingRepository ratingRepository,
        ICurrentUserContext userContext,
        IUserDirectoryClient userDirectory)
    {
        _ratingRepository = ratingRepository;
        _userContext = userContext;
        _userDirectory = userDirectory;
    }

    /// <summary>Returns the current user's rating for the story, throwing if none exists.</summary>
    public async Task<RatingResponseDto> Handle(GetMyRatingQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();

        // Look up the caller's own rating for the story; 404 if they haven't rated it.
        var rating = await _ratingRepository.GetByStoryAndUserAsync(request.StoryId, userId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.RatingNotFound);

        // Resolve the display name for the rating's author to enrich the response DTO.
        var names = await _userDirectory.GetDisplayNamesAsync(new[] { rating.UserId }, cancellationToken);

        return CommunityDtoMapper.ToDto(rating, names.TryGetValue(rating.UserId, out var name) ? name : null);
    }
}
