namespace Content.Application.Commands.Chapters.UpdateChapter;

public sealed class UpdateChapterCommandHandler : ICommandHandler<UpdateChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<UpdateChapterCommandHandler> _logger;

    public UpdateChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<UpdateChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<ChapterDetailResponseDto> Handle(UpdateChapterCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        Guid? volumePublicId = null;
        long? volumeId = null;
        if (request.VolumeId is { } requestedVolume)
        {
            var volume = await _volumeRepository.GetByPublicIdAsync(requestedVolume, cancellationToken)
                ?? throw new NotFoundException(ApplicationErrorConstants.VolumeNotFound);

            if (volume.StoryId != story.Id)
            {
                throw new BusinessRuleException(ApplicationErrorConstants.VolumeStoryMismatch);
            }

            volumeId = volume.Id;
            volumePublicId = volume.PublicId;
        }

        // Editing a live chapter in place is allowed for small fixes; flag it so
        // substantial rewrites can be moved to a versioned edit later (rules 10.8).
        if (chapter.Status == ChapterStatus.Published)
        {
            _logger.LogWarning(ApplicationLogConstants.ChapterEditedAfterPublish, chapter.Id);
        }

        chapter.Title = request.Title.Trim();
        chapter.OrderIndex = request.OrderIndex;
        chapter.Content = request.Content;
        chapter.WordCount = WordCounter.Count(request.Content);
        chapter.VolumeId = volumeId;
        chapter.UpdatedAt = DateTime.UtcNow;

        _chapterRepository.Update(chapter);
        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterUpdated, chapter.Id);

        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }
}
