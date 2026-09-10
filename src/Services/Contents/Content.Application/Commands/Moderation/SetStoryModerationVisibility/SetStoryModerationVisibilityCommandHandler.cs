namespace Content.Application.Commands.Moderation.SetStoryModerationVisibility;

public sealed class SetStoryModerationVisibilityCommandHandler
    : ICommandHandler<SetStoryModerationVisibilityCommand, ModerationVisibilityResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<SetStoryModerationVisibilityCommandHandler> _logger;

    public SetStoryModerationVisibilityCommandHandler(
        IStoryRepository storyRepository,
        ILogger<SetStoryModerationVisibilityCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _logger = logger;
    }

    public async Task<ModerationVisibilityResponseDto> Handle(
        SetStoryModerationVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Hide -> Removed. Restore -> Ongoing when the story has published content,
        // otherwise back to Draft. A restore of a story that was never Removed is a no-op.
        if (request.Hidden)
        {
            if (story.Status != StoryStatus.Removed)
            {
                story.Status = StoryStatus.Removed;
                story.UpdatedAt = DateTime.UtcNow;
                _storyRepository.Update(story);
                await _storyRepository.SaveChangesAsync(cancellationToken);
            }
        }
        else if (story.Status == StoryStatus.Removed)
        {
            story.Status = story.PublishedAt is null ? StoryStatus.Draft : StoryStatus.Ongoing;
            story.UpdatedAt = DateTime.UtcNow;
            _storyRepository.Update(story);
            await _storyRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            ApplicationLogConstants.StoryModerationVisibilityChanged, story.Id, request.Hidden, story.Status);

        return new ModerationVisibilityResponseDto { Id = story.PublicId, Status = story.Status.ToString() };
    }
}
