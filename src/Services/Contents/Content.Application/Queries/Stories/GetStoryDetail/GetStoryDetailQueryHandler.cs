namespace Content.Application.Queries.Stories.GetStoryDetail;

public sealed class GetStoryDetailQueryHandler : IQueryHandler<GetStoryDetailQuery, StoryDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ICurrentAuthorContext _authorContext;

    public GetStoryDetailQueryHandler(IStoryRepository storyRepository, ICurrentAuthorContext authorContext)
    {
        _storyRepository = storyRepository;
        _authorContext = authorContext;
    }

    public async Task<StoryDetailResponseDto> Handle(GetStoryDetailQuery request, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetBySlugAsync(request.Slug.Trim().ToLowerInvariant(), cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        var callerAuthorId = _authorContext.IsAuthor ? _authorContext.GetAuthorProfileId() : (long?)null;
        var isOwner = callerAuthorId == story.AuthorProfileId;

        if (story.Status == StoryStatus.Draft && !isOwner)
        {
            // A draft story must not be discoverable by anyone but its author.
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        var full = await _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        if (story.Status != StoryStatus.Draft && !isOwner)
        {
            // Accepted read-path side effect: opening a story is a ranking signal and
            // must be tracked independently of any payment concept
            // (product-workflow-context.md 5.4). The owner's own previews are not reads.
            // TODO: batch + de-duplicate these increments once traffic warrants it.
            await _storyRepository.IncrementViewCountAsync(story.Id, cancellationToken);
            full.ViewCount += 1;
        }

        return ContentDtoMapper.ToDetail(full);
    }
}
