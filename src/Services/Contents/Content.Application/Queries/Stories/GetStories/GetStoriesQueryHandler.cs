namespace Content.Application.Queries.Stories.GetStories;

public sealed class GetStoriesQueryHandler
    : IQueryHandler<GetStoriesQuery, PagedResponseDto<StorySummaryResponseDto>>
{
    private readonly IStoryRepository _storyRepository;

    public GetStoriesQueryHandler(IStoryRepository storyRepository)
    {
        _storyRepository = storyRepository;
    }

    public async Task<PagedResponseDto<StorySummaryResponseDto>> Handle(
        GetStoriesQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        var criteria = new StorySearchCriteria
        {
            GenreSlug = NormalizeSlug(request.GenreSlug),
            TagSlug = NormalizeSlug(request.TagSlug),
            AuthorProfileId = request.AuthorProfileId is > 0 ? request.AuthorProfileId : null,
            Status = ParseStatus(request.Status),
            ChapterLength = ParseChapterLength(request.Length),
            SortBy = ParseSortField(request.SortBy),
            Descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _storyRepository.SearchPublishedAsync(criteria, cancellationToken);

        var commentCounts = await _storyRepository.GetCommentCountsAsync(
            items.Select(x => x.Id), cancellationToken) ?? new Dictionary<long, int>();
        var chapterCounts = await _storyRepository.GetPublishedChapterCountsAsync(
            items.Select(x => x.Id), cancellationToken) ?? new Dictionary<long, int>();

        var summaries = items
            .Select(story => ContentDtoMapper.ToSummary(
                story,
                ContentDtoMapper.PrimaryGenreName(story),
                commentCounts.GetValueOrDefault(story.Id),
                chapterCounts.GetValueOrDefault(story.Id)))
            .ToArray();

        return PagedResponseDto<StorySummaryResponseDto>.Create(summaries, pageNumber, pageSize, totalCount);
    }

    private static string NormalizeSlug(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
    }

    private static StoryStatus? ParseStatus(string value)
    {
        if (Enum.TryParse<StoryStatus>(value, ignoreCase: true, out var status) && status != StoryStatus.Draft)
        {
            return status;
        }

        return null;
    }

    private static ChapterLengthFilter? ParseChapterLength(string value)
    {
        return Enum.TryParse<ChapterLengthFilter>(value, ignoreCase: true, out var length)
            ? length
            : null;
    }

    private static StorySortField ParseSortField(string value)
    {
        return Enum.TryParse<StorySortField>(value, ignoreCase: true, out var field)
            ? field
            : StorySortField.PublishedAt;
    }
}
