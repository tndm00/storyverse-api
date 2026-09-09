namespace Content.Application.Queries.Chapters.GetChapterForReview;

public sealed class GetChapterForReviewQueryHandler : IQueryHandler<GetChapterForReviewQuery, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;

    public GetChapterForReviewQueryHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
    }

    public async Task<ChapterDetailResponseDto> Handle(GetChapterForReviewQuery request, CancellationToken cancellationToken)
    {
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var volumePublicId = chapter.VolumeId is { } volumeId
            ? (await _volumeRepository.GetByIdAsync(volumeId, cancellationToken))?.PublicId
            : null;

        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }
}
