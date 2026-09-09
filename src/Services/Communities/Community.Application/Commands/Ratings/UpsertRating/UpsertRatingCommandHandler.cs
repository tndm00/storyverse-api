namespace Community.Application.Commands.Ratings.UpsertRating;

public sealed class UpsertRatingCommandHandler : ICommandHandler<UpsertRatingCommand, RatingResponseDto>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICommunityUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _userContext;
    private readonly ILogger<UpsertRatingCommandHandler> _logger;

    public UpsertRatingCommandHandler(
        IRatingRepository ratingRepository,
        ICommunityUnitOfWork unitOfWork,
        ICurrentUserContext userContext,
        ILogger<UpsertRatingCommandHandler> logger)
    {
        _ratingRepository = ratingRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task<RatingResponseDto> Handle(UpsertRatingCommand request, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetUserId();
        var reviewText = string.IsNullOrWhiteSpace(request.ReviewText) ? null : request.ReviewText.Trim();

        Rating rating = null;

        // The read-then-write must be atomic so a user double-submitting cannot
        // race two INSERTs past the unique (story_id, user_id) index.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            rating = await _ratingRepository.GetByStoryAndUserAsync(request.StoryId, userId, ct);

            if (rating is null)
            {
                rating = new Rating
                {
                    StoryId = request.StoryId,
                    UserId = userId,
                    Score = request.Score,
                    ReviewText = reviewText
                };
                await _ratingRepository.AddAsync(rating, ct);
            }
            else
            {
                rating.Score = request.Score;
                rating.ReviewText = reviewText;
                rating.UpdatedAt = DateTime.UtcNow;
                _ratingRepository.Update(rating);
            }

            await _ratingRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.RatingUpserted, rating.PublicId, rating.StoryId, rating.Score, userId);

        // Integration point: publish a "RatingChanged" event carrying the story id so
        // the Content service can recompute Story.RatingAvg / Story.RatingCount. This
        // service must never write the Content database directly. The EventBus package
        // is interfaces-only in Phase 1, so there is nothing to dispatch to yet.

        return CommunityDtoMapper.ToDto(rating);
    }
}
