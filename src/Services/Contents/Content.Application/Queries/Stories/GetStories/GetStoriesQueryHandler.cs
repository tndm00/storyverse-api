namespace Content.Application.Queries.Stories.GetStories;

public sealed class GetStoriesQueryHandler
    : IQueryHandler<GetStoriesQuery, PagedResponseDto<StorySummaryResponseDto>>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IStorySearchService _storySearchService;
    private readonly ILogger<GetStoriesQueryHandler> _logger;

    public GetStoriesQueryHandler(
        IStoryRepository storyRepository,
        IStorySearchService storySearchService,
        ILogger<GetStoriesQueryHandler> logger)
    {
        _storyRepository = storyRepository;
        _storySearchService = storySearchService;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paged listing of published stories matching the given filters, enriched with
    /// comment and chapter counts. Search is delegated to <see cref="SearchAsync"/>.
    /// </summary>
    public async Task<PagedResponseDto<StorySummaryResponseDto>> Handle(
        GetStoriesQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize paging inputs to safe bounds.
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        var criteria = new StorySearchCriteria
        {
            GenreSlug = NormalizeSlug(request.GenreSlug),
            TagSlug = NormalizeSlug(request.TagSlug),
            Keyword = string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim(),
            AuthorProfileId = request.AuthorProfileId is > 0 ? request.AuthorProfileId : null,
            Status = ParseStatus(request.Status),
            ChapterLength = ParseChapterLength(request.Length),
            SortBy = ParseSortField(request.SortBy),
            Descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await SearchAsync(criteria, cancellationToken);

        // Fetch comment/chapter counts in bulk for the returned page of stories.
        var commentCounts = await _storyRepository.GetCommentCountsAsync(
            items.Select(x => x.Id), cancellationToken) ?? new Dictionary<long, int>();
        var chapterCounts = await _storyRepository.GetPublishedChapterCountsAsync(
            items.Select(x => x.Id), cancellationToken) ?? new Dictionary<long, int>();

        // Map domain entities to summary DTOs.
        var summaries = items
            .Select(story => ContentDtoMapper.ToSummary(
                story,
                ContentDtoMapper.PrimaryGenreName(story),
                commentCounts.GetValueOrDefault(story.Id),
                chapterCounts.GetValueOrDefault(story.Id)))
            .ToArray();

        return PagedResponseDto<StorySummaryResponseDto>.Create(summaries, pageNumber, pageSize, totalCount);
    }

    /// <summary>
    /// Routes keyword search through Elasticsearch when available; otherwise
    /// (no keyword, ES disabled/not yet cut over, or an ES failure) falls back
    /// to the existing Postgres ILIKE path unchanged. See the Elasticsearch
    /// rollout plan for the staged Enabled/SearchReadEnabled cutover.
    /// </summary>
    private async Task<(IReadOnlyList<Story> Items, int TotalCount)> SearchAsync(
        StorySearchCriteria criteria, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            try
            {
                var (storyIds, totalCount) = await _storySearchService.SearchAsync(criteria, cancellationToken);
                if (storyIds.Count > 0 || totalCount > 0)
                {
                    var items = await _storyRepository.GetByIdsInOrderAsync(storyIds, cancellationToken);
                    return (items, totalCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ApplicationLogConstants.StorySearchFallback, ex.Message);
            }
        }

        return await _storyRepository.SearchPublishedAsync(criteria, cancellationToken);
    }

    /// <summary>Trims and lowercases a slug filter value, or returns null when blank.</summary>
    private static string NormalizeSlug(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    }

    /// <summary>Parses a status filter, rejecting Draft since drafts are never publicly listed.</summary>
    private static StoryStatus? ParseStatus(string value)
    {
        if (Enum.TryParse<StoryStatus>(value, ignoreCase: true, out var status) && status != StoryStatus.Draft)
        {
            return status;
        }

        return null;
    }

    /// <summary>Parses the chapter-length filter, returning null when the value is not recognized.</summary>
    private static ChapterLengthFilter? ParseChapterLength(string value)
    {
        return Enum.TryParse<ChapterLengthFilter>(value, ignoreCase: true, out var length)
            ? length
            : null;
    }

    /// <summary>Parses the sort field, defaulting to PublishedAt when the value is not recognized.</summary>
    private static StorySortField ParseSortField(string value)
    {
        return Enum.TryParse<StorySortField>(value, ignoreCase: true, out var field)
            ? field
            : StorySortField.PublishedAt;
    }
}
