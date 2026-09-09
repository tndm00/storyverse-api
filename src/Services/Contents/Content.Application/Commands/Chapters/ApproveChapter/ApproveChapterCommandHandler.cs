namespace Content.Application.Commands.Chapters.ApproveChapter;

public sealed class ApproveChapterCommandHandler : ICommandHandler<ApproveChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ILogger<ApproveChapterCommandHandler> _logger;

    public ApproveChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ILogger<ApproveChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _logger = logger;
    }

    public async Task<ChapterDetailResponseDto> Handle(ApproveChapterCommand request, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        if (!ChapterStatusPolicy.CanApprove(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        var story = await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken);
        var now = DateTime.UtcNow;

        chapter.Status = ChapterStatus.Published;
        chapter.PublishedAt ??= now;
        chapter.RejectionReason = null;
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

        _logger.LogInformation(ApplicationLogConstants.ChapterApproved, chapter.Id, story.Id);

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
