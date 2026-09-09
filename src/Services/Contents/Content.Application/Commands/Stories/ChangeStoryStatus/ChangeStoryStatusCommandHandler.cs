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

    public async Task<StoryDetailResponseDto> Handle(ChangeStoryStatusCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var story = await _storyRepository.GetWithClassificationByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        if (story.AuthorProfileId != authorProfileId)
        {
            _logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        if (!StoryStatusPolicy.CanTransitionManually(story.Status, request.TargetStatus))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidStoryStatusTransition);
        }

        var previousStatus = story.Status;
        story.Status = request.TargetStatus;
        story.UpdatedAt = DateTime.UtcNow;

        _storyRepository.Update(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryStatusChanged, story.Id, previousStatus, story.Status);

        return ContentDtoMapper.ToDetail(story);
    }
}
