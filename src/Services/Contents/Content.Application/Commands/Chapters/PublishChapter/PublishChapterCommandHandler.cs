namespace Content.Application.Commands.Chapters.PublishChapter;

public sealed class PublishChapterCommandHandler : ICommandHandler<PublishChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<PublishChapterCommandHandler> _logger;

    public PublishChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<PublishChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<ChapterDetailResponseDto> Handle(PublishChapterCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        if (!ChapterStatusPolicy.CanPublish(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        if (!await _storyRepository.HasExactlyOnePrimaryGenreAsync(story.Id, cancellationToken))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.PrimaryGenreRequired);
        }

        var now = DateTime.UtcNow;

        chapter.Status = ChapterStatus.Published;
        chapter.PublishedAt ??= now;
        chapter.ScheduledAt = null;
        chapter.UpdatedAt = now;
        _chapterRepository.Update(chapter);

        if (story.Status == StoryStatus.Draft)
        {
            story.Status = StoryStatus.Ongoing;
            story.PublishedAt ??= now;
            story.UpdatedAt = now;
            _storyRepository.Update(story);
            _logger.LogInformation(ApplicationLogConstants.StoryAutoOngoing, story.Id);
        }

        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterPublished, chapter.Id, story.Id);

        // Integration point: publish a "chapter published" event so the Notification
        // service can alert followers. No event bus implementation exists yet (Phase 1A).

        var volumePublicId = await ResolveVolumePublicIdAsync(chapter.VolumeId, cancellationToken);
        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }

    private async Task<Guid?> ResolveVolumePublicIdAsync(long? volumeId, CancellationToken cancellationToken)
    {
        if (volumeId is not { } id)
        {
            return null;
        }

        var volume = await _volumeRepository.GetByIdAsync(id, cancellationToken);
        return volume?.PublicId;
    }
}
