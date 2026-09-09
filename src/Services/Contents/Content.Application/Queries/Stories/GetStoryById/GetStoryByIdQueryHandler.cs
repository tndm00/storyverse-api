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

    public async Task<StoryDetailResponseDto> Handle(GetStoryByIdQuery request, CancellationToken cancellationToken)
    {
        var story = await _storyRepository.GetByPublicIdAsync(request.StoryId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        var callerAuthorId = _authorContext.IsAuthor ? _authorContext.GetAuthorProfileId() : (long?)null;
        var isOwner = callerAuthorId == story.AuthorProfileId;

        if (story.Status == StoryStatus.Draft && !isOwner)
        {
            throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);
        }

        var full = await _storyRepository.GetWithClassificationByPublicIdAsync(story.PublicId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.StoryNotFound);

        if (story.Status != StoryStatus.Draft && !isOwner)
        {
            await _storyRepository.IncrementViewCountAsync(story.Id, cancellationToken);
            full.ViewCount += 1;
        }

        return ContentDtoMapper.ToDetail(full);
    }
}
