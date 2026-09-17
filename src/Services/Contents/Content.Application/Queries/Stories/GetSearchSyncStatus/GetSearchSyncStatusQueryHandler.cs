namespace Content.Application.Queries.Stories.GetSearchSyncStatus;

public sealed class GetSearchSyncStatusQueryHandler
    : IQueryHandler<GetSearchSyncStatusQuery, SearchSyncStatusResponseDto>
{
    private readonly ISearchSyncCursorRepository _cursorRepository;
    private readonly IStorySearchService _storySearchService;
    private readonly IStoryRepository _storyRepository;

    public GetSearchSyncStatusQueryHandler(
        ISearchSyncCursorRepository cursorRepository,
        IStorySearchService storySearchService,
        IStoryRepository storyRepository)
    {
        _cursorRepository = cursorRepository;
        _storySearchService = storySearchService;
        _storyRepository = storyRepository;
    }

    /// <summary>
    /// Reports how far the Elasticsearch background sync job has progressed compared to
    /// the eligible (non-Draft) stories in Postgres, plus whether sync/search reads are enabled.
    /// </summary>
    public async Task<SearchSyncStatusResponseDto> Handle(
        GetSearchSyncStatusQuery request, CancellationToken cancellationToken)
    {
        // Gather sync cursor, live index document count, and Postgres status counts.
        var lastSyncedAt = await _cursorRepository.GetLastSyncedAtAsync(cancellationToken);
        var syncedDocumentCount = await _storySearchService.GetDocumentCountAsync(cancellationToken);
        var statusCounts = await _storyRepository.CountByStatusAsync(cancellationToken);

        // Draft stories are never indexed, so exclude them from the eligible target count.
        var eligibleStoryCount = statusCounts
            .Where(kv => kv.Key != StoryStatus.Draft)
            .Sum(kv => kv.Value);

        return new SearchSyncStatusResponseDto
        {
            LastSyncedAt = lastSyncedAt == DateTime.MinValue ? null : lastSyncedAt,
            SyncedDocumentCount = syncedDocumentCount,
            EligibleStoryCount = eligibleStoryCount,
            Enabled = _storySearchService.IsEnabled,
            SearchReadEnabled = _storySearchService.IsSearchReadEnabled
        };
    }
}
