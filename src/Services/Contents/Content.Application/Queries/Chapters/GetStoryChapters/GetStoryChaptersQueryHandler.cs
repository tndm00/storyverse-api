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

    /// <summary>
    /// Returns the story's table of contents. The owner sees every chapter including
    /// drafts; anyone else sees only published chapters, and a draft story is hidden entirely.
    /// </summary>
    public async Task<IReadOnlyList<ChapterSummaryResponseDto>> Handle(
        GetStoryChaptersQuery request,
        CancellationToken cancellationToken)
    {
        // Resolve the story; it must exist.
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Determine whether the caller is the story's owning author.
        var callerAuthorId = _authorContext.IsAuthor ? _authorContext.GetAuthorProfileId() : (long?)null;
        var isOwner = callerAuthorId == story.AuthorProfileId;

        // A draft story is visible only to its owner.
        if (story.Status == StoryStatus.Draft && !isOwner)
        {
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        // Non-owners only see published chapters.
        var chapters = await _chapterRepository.GetByStoryAsync(story.Id, publishedOnly: !isOwner, cancellationToken);

        // Build a lookup from volume id to its public id for the response mapping.
        var volumePublicIdById = (await _volumeRepository.GetByStoryAsync(story.Id, cancellationToken))
            .ToDictionary(v => v.Id, v => v.PublicId);

        // Order chapters for display and map to summary DTOs.
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
