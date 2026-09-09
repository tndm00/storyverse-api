namespace Content.Application.Commands.Chapters.ReviewChapter;

public sealed class ReviewChapterCommandHandler : ICommandHandler<ReviewChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _currentUser;
    private readonly ILogger<ReviewChapterCommandHandler> _logger;

    public ReviewChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext currentUser,
        ILogger<ReviewChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ChapterDetailResponseDto> Handle(ReviewChapterCommand request, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        if (!ChapterStatusPolicy.CanStartReview(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        chapter.Status = ChapterStatus.InReview;
        chapter.UpdatedAt = DateTime.UtcNow;
        _chapterRepository.Update(chapter);
        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterReviewStarted, chapter.Id, _currentUser.GetUserId());

        var story = await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken);
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
