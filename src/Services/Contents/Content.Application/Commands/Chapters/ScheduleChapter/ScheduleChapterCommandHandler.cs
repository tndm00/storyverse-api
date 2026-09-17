namespace Content.Application.Commands.Chapters.ScheduleChapter;

public sealed class ScheduleChapterCommandHandler : ICommandHandler<ScheduleChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<ScheduleChapterCommandHandler> _logger;

    public ScheduleChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<ScheduleChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>
    /// Schedules a draft chapter to publish automatically at a future time.
    /// Requires the caller to own the parent story.
    /// </summary>
    public async Task<ChapterDetailResponseDto> Handle(ScheduleChapterCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Look up the chapter and verify the caller owns its story.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        if (!ChapterStatusPolicy.CanSchedule(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        // Move the chapter to Scheduled with the requested publish time.
        chapter.Status = ChapterStatus.Scheduled;
        chapter.ScheduledAt = request.ScheduledAt;
        chapter.UpdatedAt = DateTime.UtcNow;

        _chapterRepository.Update(chapter);
        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterScheduled, chapter.Id, request.ScheduledAt);

        // Map to the response DTO, resolving the volume's public id if any.
        var volumePublicId = chapter.VolumeId is { } id
            ? (await _volumeRepository.GetByIdAsync(id, cancellationToken))?.PublicId
            : null;

        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }
}
