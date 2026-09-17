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

    public async Task<SearchSyncStatusResponseDto> Handle(
        GetSearchSyncStatusQuery request, CancellationToken cancellationToken)
    {
        var lastSyncedAt = await _cursorRepository.GetLastSyncedAtAsync(cancellationToken);
        var syncedDocumentCount = await _storySearchService.GetDocumentCountAsync(cancellationToken);
        var statusCounts = await _storyRepository.CountByStatusAsync(cancellationToken);

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
