namespace Content.Application.Queries.Stories.GetAdminStories;

public sealed class GetAdminStoriesQueryHandler
    : IQueryHandler<GetAdminStoriesQuery, PagedResponseDto<StorySummaryResponseDto>>
{
    private readonly IStoryRepository _storyRepository;

    public GetAdminStoriesQueryHandler(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    /// <summary>
    /// Returns a paged, filterable admin catalog listing of stories across every status.
    /// </summary>
    public async Task<PagedResponseDto<StorySummaryResponseDto>> Handle(
        GetAdminStoriesQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize paging inputs to safe bounds.
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        // Build search criteria, normalizing/parsing optional filter and sort inputs.
        var criteria = new StorySearchCriteria
        {
            GenreSlug = string.IsNullOrWhiteSpace(request.GenreSlug)
                ? null
                : request.GenreSlug.Trim().ToLowerInvariant(),
            Keyword = string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim(),
            AuthorProfileId = request.AuthorProfileId is > 0 ? request.AuthorProfileId : null,
            Status = Enum.TryParse<StoryStatus>(request.Status, ignoreCase: true, out var status) ? status : null,
            SortBy = Enum.TryParse<StorySortField>(request.SortBy, ignoreCase: true, out var field)
                ? field
                : StorySortField.CreatedAt,
            Descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _storyRepository.SearchAllAsync(criteria, cancellationToken);

        // Map stories to summary DTOs, resolving each story's primary genre name.
        var summaries = items
            .Select(story => ContentDtoMapper.ToSummary(story, ContentDtoMapper.PrimaryGenreName(story)))
            .ToArray();

        return PagedResponseDto<StorySummaryResponseDto>.Create(summaries, pageNumber, pageSize, totalCount);
    }
}
