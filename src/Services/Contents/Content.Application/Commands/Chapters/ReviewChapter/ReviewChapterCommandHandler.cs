namespace Content.Application.Commands.Chapters.ReviewChapter;

public sealed class ReviewChapterCommandHandler : ICommandHandler<ReviewChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly IChapterReviewActionRepository _reviewActionRepository;
    private readonly IContentUnitOfWork _unitOfWork;
    private readonly ICurrentAuthorContext _currentUser;
    private readonly ILogger<ReviewChapterCommandHandler> _logger;

    public ReviewChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        IChapterReviewActionRepository reviewActionRepository,
        IContentUnitOfWork unitOfWork,
        ICurrentAuthorContext currentUser,
        ILogger<ReviewChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _reviewActionRepository = reviewActionRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Moves a chapter from the pending-review queue into active review by a
    /// moderator, recording the action alongside the status change.
    /// </summary>
    public async Task<ChapterDetailResponseDto> Handle(ReviewChapterCommand request, CancellationToken cancellationToken)
    {
        var moderatorUserId = _currentUser.GetUserId();

        // Look up the chapter and ensure it is currently eligible to start review.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        if (!ChapterStatusPolicy.CanStartReview(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        var now = DateTime.UtcNow;

        // Move the chapter into active review.
        chapter.Status = ChapterStatus.InReview;
        chapter.UpdatedAt = now;
        _chapterRepository.Update(chapter);

        var action = new ChapterReviewAction
        {
            ChapterId = chapter.Id,
            ModeratorUserId = moderatorUserId,
            Action = ChapterReviewActionType.Reviewed,
            Note = null,
            CreatedAt = now
        };

        // The chapter status change and its audit row must commit together.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _reviewActionRepository.AddAsync(action, ct);
            await _chapterRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterReviewStarted, chapter.Id, moderatorUserId);

        // Map to the response DTO, resolving the volume's public id if any.
        var story = await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken);
        var volumePublicId = await ResolveVolumePublicIdAsync(chapter.VolumeId, cancellationToken);
        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }

    /// <summary>
    /// Resolves a volume's public id from its internal id, if the chapter belongs to one.
    /// </summary>
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
