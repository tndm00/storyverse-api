using Content.Application.Options;
using Microsoft.Extensions.Options;

namespace Content.Application.Commands.Stories.SyncStorySearchIndex;

public sealed class SyncStorySearchIndexCommandHandler
    : ICommandHandler<SyncStorySearchIndexCommand, SyncStorySearchIndexResultDto>
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;
    private readonly ISearchSyncCursorRepository _cursorRepository;
    private readonly IStorySearchService _storySearchService;
    private readonly StorySearchSyncOptions _options;
    private readonly ILogger<SyncStorySearchIndexCommandHandler> _logger;

    public SyncStorySearchIndexCommandHandler(
        IStoryRepository storyRepository,
        IChapterRepository chapterRepository,
        ISearchSyncCursorRepository cursorRepository,
        IStorySearchService storySearchService,
        IOptions<StorySearchSyncOptions> options,
        ILogger<SyncStorySearchIndexCommandHandler> logger)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
        _cursorRepository = cursorRepository;
        _storySearchService = storySearchService;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Advances the search-sync cursor and (re)indexes every story changed since the last run: drafts are
    /// removed from the index, everything else is indexed with its currently published chapter content.
    /// </summary>
    public async Task<SyncStorySearchIndexResultDto> Handle(SyncStorySearchIndexCommand request, CancellationToken cancellationToken)
    {
        // Captured before querying: everything up to this instant is covered by
        // this run's <= bound, so the cursor can safely advance to exactly this
        // value afterward, however many rows were actually found.
        var runStartedAt = DateTime.UtcNow;
        var lastSyncedAt = await _cursorRepository.GetLastSyncedAtAsync(cancellationToken);

        // Find every story that changed since the previous sync run.
        var changedStoryIds = await _storyRepository.GetStoryIdsChangedBetweenAsync(
            lastSyncedAt, runStartedAt, _options.BatchSize, cancellationToken);

        if (changedStoryIds.Count > 0)
        {
            var stories = await _storyRepository.GetByIdsInOrderAsync(changedStoryIds, cancellationToken);

            foreach (var story in stories)
            {
                // Drafts are never searchable, so remove them from the index instead of indexing them.
                if (story.Status == StoryStatus.Draft)
                {
                    await _storySearchService.DeleteAsync(story.Id, cancellationToken);
                    continue;
                }

                var publishedContent = await _chapterRepository.GetPublishedContentByStoryIdAsync(story.Id, cancellationToken);
                await _storySearchService.IndexAsync(story, publishedContent, cancellationToken);
            }
        }

        // Advance the cursor so the next run only looks at changes after this point.
        await _cursorRepository.SetLastSyncedAtAsync(runStartedAt, cancellationToken);

        if (changedStoryIds.Count > 0)
        {
            _logger.LogInformation(ApplicationLogConstants.StorySearchSyncCompleted, changedStoryIds.Count);
        }

        return new SyncStorySearchIndexResultDto { ChangedStoryCount = changedStoryIds.Count };
    }
}
