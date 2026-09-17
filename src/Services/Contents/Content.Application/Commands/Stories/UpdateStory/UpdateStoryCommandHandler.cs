namespace Content.Application.Commands.Stories.UpdateStory;

public sealed class UpdateStoryCommandHandler : ICommandHandler<UpdateStoryCommand, StoryDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<UpdateStoryCommandHandler> _logger;

    public UpdateStoryCommandHandler(
        IStoryRepository storyRepository,
        ICurrentAuthorContext authorContext,
        ILogger<UpdateStoryCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>Updates a story's editable metadata after verifying the caller owns it. Status and classification are untouched.</summary>
    public async Task<StoryDetailResponseDto> Handle(UpdateStoryCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Load the story with its classification data (genres/tags) included.
        var story = await _storyRepository.GetWithClassificationByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Only the owning author may update the story.
        if (story.AuthorProfileId != authorProfileId)
        {
            _logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        // Apply the editable metadata fields.
        story.Title = request.Title.Trim();
        story.Description = request.Description;
        story.CoverImageUrl = request.CoverImageUrl;
        story.ContentType = request.ContentType;
        story.OriginalSource = request.ContentType == StoryContentType.Translated ? request.OriginalSource : null;
        story.Language = string.IsNullOrWhiteSpace(request.Language) ? ApplicationConstants.DefaultLanguage : request.Language.Trim();
        story.AgeRating = request.AgeRating;
        story.UpdatedAt = DateTime.UtcNow;

        // Persist the change.
        _storyRepository.Update(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryUpdated, story.Id);

        return ContentDtoMapper.ToDetail(story);
    }
}
