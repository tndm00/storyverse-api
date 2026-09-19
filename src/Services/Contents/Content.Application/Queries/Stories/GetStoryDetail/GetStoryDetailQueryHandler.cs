namespace Content.Application.Queries.Stories.GetStoryDetail;

public sealed class GetStoryDetailQueryHandler : IQueryHandler<GetStoryDetailQuery, StoryDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ICurrentAuthorContext _authorContext;
    private readonly IViewTracker _viewTracker;

    public GetStoryDetailQueryHandler(
        IStoryRepository storyRepository,
        ICurrentAuthorContext authorContext,
        IViewTracker viewTracker)
    {
        _storyRepository = storyRepository;
        _authorContext = authorContext;
        _viewTracker = viewTracker;
    }

    /// <summary>
    /// Loads a story's page header by slug, enforcing that a draft story is visible only
    /// to its owner, and increments the view count on non-owner published reads.
    /// </summary>
    public async Task<StoryDetailResponseDto> Handle(GetStoryDetailQuery request, CancellationToken cancellationToken)
    {
        // Resolve the story by its normalized slug; it must exist.
        var story = await _storyRepository.GetBySlugAsync(request.Slug.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Determine whether the caller is the story's owning author.
        var callerAuthorId = _authorContext.IsAuthor ? _authorContext.GetAuthorProfileId() : (long?)null;
        var isOwner = callerAuthorId == story.AuthorProfileId;

        if (story.Status == StoryStatus.Draft && !isOwner)
        {
            // A draft story must not be discoverable by anyone but its author.
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        // Load the full detail including classification data.
        var full = await _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        if (story.Status != StoryStatus.Draft && !isOwner)
        {
            // Accepted read-path side effect: opening a story is a ranking signal and
            // must be tracked independently of any payment concept
            // (product-workflow-context.md 5.4). The owner's own previews are not reads.
            // Buffered in Redis and flushed to Postgres in batches (falls back to a direct increment
            // if Redis is unavailable); de-duplicating repeat views is still an open TODO.
            await _viewTracker.RecordStoryViewAsync(story.Id, cancellationToken);
            full.ViewCount += 1;
        }

        return ContentDtoMapper.ToDetail(full);
    }
}
