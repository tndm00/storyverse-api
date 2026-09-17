namespace Content.Application.Queries.Chapters.GetChapterForReview;

public sealed class GetChapterForReviewQueryHandler : IQueryHandler<GetChapterForReviewQuery, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly IChapterReviewActionRepository _reviewActionRepository;

    public GetChapterForReviewQueryHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        IChapterReviewActionRepository reviewActionRepository)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _reviewActionRepository = reviewActionRepository;
    }

    /// <summary>
    /// Loads full chapter detail plus its review action history for a moderator,
    /// regardless of chapter status or ownership.
    /// </summary>
    public async Task<ChapterDetailResponseDto> Handle(GetChapterForReviewQuery request, CancellationToken cancellationToken)
    {
        // Resolve the chapter and its parent story; both must exist.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        // Resolve the volume's public id, if the chapter belongs to one.
        var volumePublicId = chapter.VolumeId is { } volumeId
            ? (await _volumeRepository.GetByIdAsync(volumeId, cancellationToken))?.PublicId
            : null;

        // Include the chapter's moderation review history in the response.
        var reviewActions = await _reviewActionRepository.GetByChapterIdAsync(chapter.Id, cancellationToken);

        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId, reviewActions);
    }
}
