namespace Content.Application.Queries.Stories.GetMyStories;

public sealed class GetMyStoriesQueryHandler
    : IQueryHandler<GetMyStoriesQuery, PagedResponseDto<StorySummaryResponseDto>>
{
    private readonly IStoryRepository _storyRepository;
    private readonly ICurrentAuthorContext _authorContext;

    public GetMyStoriesQueryHandler(IStoryRepository storyRepository, ICurrentAuthorContext authorContext)
    {
        _storyRepository = storyRepository;
        _authorContext = authorContext;
    }

    public async Task<PagedResponseDto<StorySummaryResponseDto>> Handle(
        GetMyStoriesQuery request,
        CancellationToken cancellationToken)
    {
        // Throws ForbiddenException when the caller has no author profile.
        var authorProfileId = _authorContext.GetAuthorProfileId();

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        var criteria = new StorySearchCriteria
        {
            AuthorProfileId = authorProfileId,
            Status = Enum.TryParse<StoryStatus>(request.Status, ignoreCase: true, out var status) ? status : null,
            SortBy = Enum.TryParse<StorySortField>(request.SortBy, ignoreCase: true, out var field)
                ? field
                : StorySortField.CreatedAt,
            Descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _storyRepository.SearchAllAsync(criteria, cancellationToken);

        var summaries = items
            .Select(story => ContentDtoMapper.ToSummary(story, ContentDtoMapper.PrimaryGenreName(story)))
            .ToArray();

        return PagedResponseDto<StorySummaryResponseDto>.Create(summaries, pageNumber, pageSize, totalCount);
    }
}
