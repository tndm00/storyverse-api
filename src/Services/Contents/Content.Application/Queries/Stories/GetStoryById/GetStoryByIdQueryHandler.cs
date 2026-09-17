namespace Content.Application.Queries.Stories.GetStoryById;

public sealed class GetStoryByIdQueryHandler : IQueryHandler<GetStoryByIdQuery, StoryDetailResponseDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ICurrentAuthorContext _authorContext;

    public GetStoryByIdQueryHandler(IStoryRepository storyRepository, ICurrentAuthorContext authorContext)
    {
        _storyRepository = storyRepository;
        _authorContext = authorContext;
    }

    /// <summary>
    /// Loads a story's page header, enforcing that a draft story is visible only to its
    /// owner or a privileged moderator, and increments the view count on non-owner published reads.
    /// </summary>
    public async Task<StoryDetailResponseDto> Handle(GetStoryByIdQuery request, CancellationToken cancellationToken)
    {
        // Resolve the story; it must exist.
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // Determine whether the caller is the story's owning author.
        var callerAuthorId = _authorContext.IsAuthor ? _authorContext.GetAuthorProfileId() : (long?)null;
        var isOwner = callerAuthorId == story.AuthorProfileId;

        // Staff with content.moderate see any story (Draft included) for review/admin.
        var isPrivileged = _authorContext.HasPermission(
            Be.StoryVerse.Shared.Authorization.StoryVersePermissions.Content.Moderate);

        // A draft story is hidden from anyone but its owner or a privileged moderator.
        if (story.Status == StoryStatus.Draft && !isOwner && !isPrivileged)
        {
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        // Load the full detail including classification data.
        var full = await _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        // A read is a ranking signal; count it only for non-owner, non-privileged reads of a published story.
        if (story.Status != StoryStatus.Draft && !isOwner && !isPrivileged)
        {
            await _storyRepository.IncrementViewCountAsync(story.Id, cancellationToken);
            full.ViewCount += 1;
        }

        return ContentDtoMapper.ToDetail(full);
    }
}
