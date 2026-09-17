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

    /// <summary>Hard-deletes a story, but only while it is still a draft; the owner or a content moderator may do so.</summary>
    public async Task<Unit> Handle(DeleteStoryCommand request, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Only drafts can be hard-deleted; anything published must go through the status-change flow.
        if (story.Status != StoryStatus.Draft)
        {
            throw new BusinessRuleException(ApplicationErrorConstants.OnlyDraftStoryCanBeDeleted);
        }

        // Moderators bypass the ownership check; everyone else must own the story.
        if (!_authorContext.HasPermission(Be.StoryVerse.Shared.Authorization.StoryVersePermissions.Content.Moderate))
        {
            StoryOwnership.EnsureOwned(story, _authorContext.GetAuthorProfileId(), _logger);
        }

        // Permanently remove the story.
        _storyRepository.Remove(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryDeleted, story.Id);

        return Unit.Value;
    }
}
