namespace Community.Application.Commands.Ratings.UpsertRating;

public sealed class UpsertRatingCommandHandler : ICommandHandler<UpsertRatingCommand, RatingResponseDto>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly ICommunityUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _userContext;
    private readonly IContentRatingSyncClient _contentRatingSyncClient;
    private readonly ILogger<UpsertRatingCommandHandler> _logger;

    public UpsertRatingCommandHandler(
        IRatingRepository ratingRepository,
        ICommunityUnitOfWork unitOfWork,
        ICurrentUserContext userContext,
        IContentRatingSyncClient contentRatingSyncClient,
        ILogger<UpsertRatingCommandHandler> logger)
    {
        _ratingRepository = ratingRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
        _contentRatingSyncClient = contentRatingSyncClient;
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

        // Sync the real aggregate to Content outside the transaction (it already
        // committed above) so Story.RatingAvg/RatingCount reflect every rating,
        // not just this one. Recomputed from the DB, never accumulated in memory.
        // Best-effort by contract (see IContentRatingSyncClient): a sync failure
        // is logged there and never propagates here.
        var (averageScore, ratingCount) = await _ratingRepository.GetAggregateByStoryAsync(
            request.StoryId, cancellationToken);
        await _contentRatingSyncClient.SyncRatingSummaryAsync(
            request.StoryId, averageScore, ratingCount, cancellationToken);

        return CommunityDtoMapper.ToDto(rating);
    }
}
