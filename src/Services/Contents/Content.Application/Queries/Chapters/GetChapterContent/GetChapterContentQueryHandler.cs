namespace Content.Application.Queries.Chapters.GetChapterContent;

public sealed class GetChapterContentQueryHandler : IQueryHandler<GetChapterContentQuery, ChapterDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly IVolumeRepository _volumeRepository;
    private readonly ICurrentAuthorContext _authorContext;

    public GetChapterContentQueryHandler(
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
    /// Loads a chapter for reading, enforcing that unpublished chapters are visible only
    /// to their owning author, and increments the view count on non-owner published reads.
    /// </summary>
    public async Task<ChapterDetailResponseDto> Handle(GetChapterContentQuery request, CancellationToken cancellationToken)
    {
        // Resolve the chapter and its parent story; both must exist.
        var chapter = await _chapterRepository.GetByPublicIdAsync(request.ChapterId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        var story = await _storyRepository.GetByIdAsync(chapter.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);

        // Determine whether the caller is the story's owning author.
        var callerAuthorId = _authorContext.IsAuthor ? _authorContext.GetAuthorProfileId() : (long?)null;
        var isOwner = callerAuthorId == story.AuthorProfileId;

        if (chapter.Status != ChapterStatus.Published)
        {
            // Unpublished/removed content is visible only to its author.
            if (!isOwner)
            {
                throw new NotFoundException(ApplicationErrorConstants.ChapterNotFound);
            }
        }
        else if (!isOwner)
        {
            // Accepted read-path side effect: a read is a ranking signal and must be
            // tracked independently of any payment concept (product-workflow-context.md 5.4).
            // Bumps the chapter and its parent story. The author's own reads are not counted.
            // TODO: batch + de-duplicate these increments once traffic warrants it.
            await _chapterRepository.IncrementViewCountAsync(chapter.Id, story.Id, cancellationToken);
            chapter.ViewCount += 1;
        }

        // Resolve the volume's public id, if the chapter belongs to one.
        var volumePublicId = chapter.VolumeId is { } volumeId
            ? (await _volumeRepository.GetByIdAsync(volumeId, cancellationToken))?.PublicId
            : null;

        return ContentDtoMapper.ToDetail(chapter, story.PublicId, volumePublicId);
    }
}
