namespace Content.Application.Commands.Ratings.SetStoryRatingSummary;

public sealed class SetStoryRatingSummaryCommandHandler
    : ICommandHandler<SetStoryRatingSummaryCommand, RatingSummaryResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<SetStoryRatingSummaryCommandHandler> _logger;

    /// <summary>Initializes the handler with the story repository and logger it depends on.</summary>
    public SetStoryRatingSummaryCommandHandler(
        IStoryRepository storyRepository,
        ILogger<SetStoryRatingSummaryCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _logger = logger;
    }

    /// <summary>Overwrites the story's denormalized rating average and count with the caller's recomputed aggregate.</summary>
    public async Task<RatingSummaryResponseDto> Handle(
        SetStoryRatingSummaryCommand request,
        CancellationToken cancellationToken)
    {
        // Look up the story by its public id.
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        var ratingAvg = Math.Round(request.RatingAvg, 2, MidpointRounding.AwayFromZero);

        // Update the denormalized rating fields and persist.
        story.RatingAvg = ratingAvg;
        story.RatingCount = request.RatingCount;
        story.UpdatedAt = DateTime.UtcNow;
        _storyRepository.Update(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.StoryRatingSummaryUpdated, story.Id, ratingAvg, request.RatingCount);

        return new RatingSummaryResponseDto
        {
            Id = story.PublicId,
            RatingAvg = story.RatingAvg,
            RatingCount = story.RatingCount
        };
    }
}
