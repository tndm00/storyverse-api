namespace Content.Application.Commands.Stories.ChangeStoryStatus;

public sealed class ChangeStoryStatusCommandHandler : ICommandHandler<ChangeStoryStatusCommand, StoryDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<ChangeStoryStatusCommandHandler> _logger;

    public ChangeStoryStatusCommandHandler(
        IStoryRepository storyRepository,
        ICurrentAuthorContext authorContext,
        ILogger<ChangeStoryStatusCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>Transitions a story to a new status after checking ownership and that the transition is allowed by the status policy.</summary>
    public async Task<StoryDetailResponseDto> Handle(ChangeStoryStatusCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Load the story with its classification data (genres/tags) included.
        var story = await _storyRepository.GetWithClassificationByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Only the owning author may change the story status.
        if (story.AuthorProfileId != authorProfileId)
        {
            _logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        // The target status must be reachable from the current status via a manual transition.
        if (!StoryStatusPolicy.CanTransitionManually(story.Status, request.TargetStatus))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidStoryStatusTransition);
        }

        var previousStatus = story.Status;
        story.Status = request.TargetStatus;
        story.UpdatedAt = DateTime.UtcNow;

        // Persist the status change.
        _storyRepository.Update(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryStatusChanged, story.Id, previousStatus, story.Status);

        return ContentDtoMapper.ToDetail(story);
    }
}
