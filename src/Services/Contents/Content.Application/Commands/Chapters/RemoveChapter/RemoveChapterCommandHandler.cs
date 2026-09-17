namespace Content.Application.Commands.Chapters.RemoveChapter;

public sealed class RemoveChapterCommandHandler : ICommandHandler<RemoveChapterCommand, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly ILogger<RemoveChapterCommandHandler> _logger;

    public RemoveChapterCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext,
        ILogger<RemoveChapterCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
        _logger = logger;
    }

    /// <summary>
    /// Removes a chapter from public view by its owning author. Requires the
    /// chapter to be in a removable status.
    /// </summary>
    public async Task<ChapterDetailResponseDto> Handle(RemoveChapterCommand request, CancellationToken cancellationToken)
    {
        var authorProfileId = _authorContext.GetAuthorProfileId();

        // Look up the chapter and verify the caller owns its story.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = StoryOwnership.EnsureOwned(
            await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken),
            authorProfileId,
            _logger);

        if (!ChapterStatusPolicy.CanRemove(chapter.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.InvalidChapterStatusTransition);
        }

        // Mark the chapter as removed.
        chapter.Status = ChapterStatus.Removed;
        chapter.UpdatedAt = DateTime.UtcNow;

        _chapterRepository.Update(chapter);
        await _chapterRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ChapterRemoved, chapter.Id);

        // Map to the response DTO, resolving the volume's public id if any.
        var volumePublicId = chapter.VolumeId is { } id
            ? (await _volumeRepository.GetByIdAsync(id, cancellationToken))?.PublicId
            : null;

        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }
}
