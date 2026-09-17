namespace Content.Application.Commands.Chapters.CancelChapterSchedule;

public sealed class CancelChapterScheduleCommandHandler
    : ICommandHandler<CancelChapterScheduleCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<CancelChapterScheduleCommandHandler> _logger;

    public CancelChapterScheduleCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<CancelChapterScheduleCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>
    /// Cancels a chapter's pending schedule and returns it to Draft. Requires
    /// the caller to own the parent story.
    /// </summary>
    public async Task<ChapterDetailResponseDto> Handle(
        CancelChapterScheduleCommand request,
        CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Look up the chapter and verify the caller owns its story.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        if (!ChapterStatusPolicy.CanCancelSchedule(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        // Revert to Draft and clear the schedule.
        chapter.Status = ChapterStatus.Draft;
        chapter.ScheduledAt = null;
        chapter.UpdatedAt = DateTime.UtcNow;

        _chapterRepository.Update(chapter);
        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterScheduleCancelled, chapter.Id);

        // Map to the response DTO, resolving the volume's public id if any.
        var volumePublicId = chapter.VolumeId is { } id
            ? (await _volumeRepository.GetByIdAsync(id, cancellationToken))?.PublicId
            : null;

        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }
}
