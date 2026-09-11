namespace Content.Application.Commands.Stories.DeleteStory;

public sealed class DeleteStoryCommandHandler : ICommandHandler<DeleteStoryCommand, Unit>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<DeleteStoryCommandHandler> _logger;

    public DeleteStoryCommandHandler(
        IStoryRepository storyRepository,
        ICurrentAuthorContext authorContext,
        ILogger<DeleteStoryCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        if (story.Status != StoryStatus.Draft)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.OnlyDraftStoryCanBeDeleted);
        }

        if (!_authorContext.HasPermission(Be.StoryVerse.Shared.Authorization.StoryVersePermissions.Content.Moderate))
        {
            StoryOwnership.EnsureOwned(story, _authorContext.GetAuthorProfileId(), _logger);
        }

        _storyRepository.Remove(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryDeleted, story.Id);

        return Unit.Value;
    }
}
