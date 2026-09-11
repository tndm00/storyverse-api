namespace Content.Application.Commands.Ratings.SetStoryRatingSummary;

public sealed class SetStoryRatingSummaryCommandHandler
    : ICommandHandler<SetStoryRatingSummaryCommand, RatingSummaryResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<SetStoryRatingSummaryCommandHandler> _logger;

    public SetStoryRatingSummaryCommandHandler(
        IStoryRepository storyRepository,
        ILogger<SetStoryRatingSummaryCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _logger = logger;
    }

    public async Task<RatingSummaryResponseDto> Handle(
        SetStoryRatingSummaryCommand request,
        CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        var ratingAvg = Math.Round(request.RatingAvg, 2, MidpointRounding.AwayFromZero);

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
