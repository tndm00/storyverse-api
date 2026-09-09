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

    public async Task<StoryDetailResponseDto> Handle(UpdateStoryCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var story = await _storyRepository.GetWithClassificationByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        if (story.AuthorProfileId != authorProfileId)
        {
            _logger.LogWarning(ApplicationLogConstants.OwnershipCheckFailed, authorProfileId, story.Id);
            throw new ForbiddenException(ApplicationErrorConstants.NotStoryOwner);
        }

        story.Title = request.Title.Trim();
        story.Description = request.Description;
        story.CoverImageUrl = request.CoverImageUrl;
        story.ContentType = request.ContentType;
        story.OriginalSource = request.ContentType == StoryContentType.Translated ? request.OriginalSource : null;
        story.Language = string.IsNullOrWhiteSpace(request.Language) ? ApplicationConstants.DefaultLanguage : request.Language.Trim();
        story.AgeRating = request.AgeRating;
        story.UpdatedAt = DateTime.UtcNow;

        _storyRepository.Update(story);
        await _storyRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.StoryUpdated, story.Id);

        return ContentDtoMapper.ToDetail(story);
    }
}
