namespace Content.Application.Queries.Chapters.GetStoryChapters;

public sealed class GetStoryChaptersQueryHandler
    : IQueryHandler<GetStoryChaptersQuery, IReadOnlyList<ChapterSummaryResponseDto>>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;

    public GetStoryChaptersQueryHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        IVolumeRepository volumeRepository,
        ICurrentAuthorContext authorContext)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _volumeRepository = volumeRepository;
        _authorContext = authorContext;
    }

    public async Task<IReadOnlyList<ChapterSummaryResponseDto>> Handle(
        GetStoryChaptersQuery request,
        CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        var callerAuthorId = _authorContext.IsAuthor ? _authorContext.GetAuthorProfileId() : (long?)null;
        var isOwner = callerAuthorId == story.AuthorProfileId;

        if (story.Status == StoryStatus.Draft && !isOwner)
        {
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        var chapters = await _chapterRepository.GetByStoryAsync(story.Id, publishedOnly: !isOwner, cancellationToken);

        var volumePublicIdById = (await _volumeRepository.GetByStoryAsync(story.Id, cancellationToken))
            .ToDictionary(v => v.Id, v => v.PublicId);

        return chapters
            .OrderBy(c => c.OrderIndex)
            .Select(c => ContentDtoMapper.ToSummary(
                c,
                c.VolumeId is { } volumeId && volumePublicIdById.TryGetValue(volumeId, out var publicId)
                    ? publicId
                    : null))
            .ToArray();
    }
}
