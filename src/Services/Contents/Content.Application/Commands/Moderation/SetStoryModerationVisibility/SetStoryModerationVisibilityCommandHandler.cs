namespace Content.Application.Commands.Moderation.SetStoryModerationVisibility;

public sealed class SetStoryModerationVisibilityCommandHandler
    : ICommandHandler<SetStoryModerationVisibilityCommand, ModerationVisibilityResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ILogger<SetStoryModerationVisibilityCommandHandler> _logger;

    /// <summary>Initializes the handler with the story repository and logger it depends on.</summary>
    public SetStoryModerationVisibilityCommandHandler(
        IStoryRepository storyRepository,
        ILogger<SetStoryModerationVisibilityCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _logger = logger;
    }

    /// <summary>
    /// Applies a moderation Hide/Remove or restore decision to a story. Restoring returns the
    /// story to Ongoing if it has published content, otherwise back to Draft.
    /// </summary>
    public async Task<ModerationVisibilityResponseDto> Handle(
        SetStoryModerationVisibilityCommand request,
        CancellationToken cancellationToken)
    {
        // Look up the story by its public id.
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Hide -> Removed. Restore -> Ongoing when the story has published content,
        // otherwise back to Draft. A restore of a story that was never Removed is a no-op.
        if (request.Hidden)
        {
            if (story.Status != StoryStatus.Removed)
            {
                // Persist the moderator's takedown.
                story.Status = StoryStatus.Removed;
                story.UpdatedAt = DateTime.UtcNow;
                _storyRepository.Update(story);
                await _storyRepository.SaveChangesAsync(cancellationToken);
            }
        }
        else if (story.Status == StoryStatus.Removed)
        {
            // Persist the restore, choosing the correct pre-removal-equivalent status.
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
