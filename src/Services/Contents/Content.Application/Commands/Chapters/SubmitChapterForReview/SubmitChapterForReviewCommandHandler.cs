namespace Content.Application.Commands.Chapters.SubmitChapterForReview;

public sealed class SubmitChapterForReviewCommandHandler
    : ICommandHandler<SubmitChapterForReviewCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<SubmitChapterForReviewCommandHandler> _logger;

    public SubmitChapterForReviewCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<SubmitChapterForReviewCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    public async Task<ChapterDetailResponseDto> Handle(
        SubmitChapterForReviewCommand request,
        CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        if (!ChapterStatusPolicy.CanSubmitForReview(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        if (!await _storyRepository.HasExactlyOnePrimaryGenreAsync(story.Id, cancellationToken))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.PrimaryGenreRequired);
        }

        chapter.Status = ChapterStatus.PendingReview;
        chapter.RejectionReason = null;
        chapter.ScheduledAt = null;
        chapter.UpdatedAt = DateTime.UtcNow;
        _chapterRepository.Update(chapter);

        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterSubmittedForReview, chapter.Id, story.Id);

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
