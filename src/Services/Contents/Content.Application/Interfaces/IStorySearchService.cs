namespace Content.Application.Interfaces;

/// <summary>
/// Elasticsearch-backed keyword search over stories, kept in sync with
/// Postgres via dual-write from the command handlers that change a story's
/// public content (title/description/published chapter text). Postgres via
/// <see cref="Repositories.IStoryRepository"/> remains the source of truth —
/// this index only ever returns story ids for <see cref="GetStoriesQueryHandler"/>
/// to hydrate, never full entity data.
/// </summary>
public interface IStorySearchService
{
    /// <summary>
    /// Indexes or re-indexes a story. <paramref name="publishedChapterContent"/>
    /// is the story's Published chapters' text, already concatenated in
    /// reading order — see
    /// <see cref="Repositories.IChapterRepository.GetPublishedContentByStoryIdAsync"/>.
    /// Draft stories are skipped internally (never returned by public search,
    /// no content worth indexing yet) — callers do not need to check this.
    /// </summary>
    Task IndexAsync(Story story, string publishedChapterContent, CancellationToken cancellationToken = default);

    Task DeleteAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Keyword search matching <see cref="StorySearchCriteria.Keyword"/> plus
    /// the same filters/sort the Postgres path applies. Returns matching story
    /// ids in ranked order and the total match count for pagination.
    /// </summary>
    Task<(IReadOnlyList<long> StoryIds, int TotalCount)> SearchAsync(
        StorySearchCriteria criteria, CancellationToken cancellationToken = default);
}
