namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="Story"/> and its genre/tag join rows.
/// Implemented by Content.Infrastructure, per code-standard.md section 28.
/// </summary>
public interface IStoryRepository
{
    Task<Story> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Story> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<Story> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Public id -&gt; title for a set of stories, for internal cross-service lookups. Unknown ids are omitted.</summary>
    Task<IReadOnlyList<ContentTitleEntryDto>> GetTitlesByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default);

    /// <summary>Loads the story with its <see cref="Story.Genres"/> and <see cref="Story.Tags"/> tracked for update.</summary>
    Task<Story> GetWithClassificationByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>True when the author already owns a story with this exact title; guards quick-publish double-submit.</summary>
    Task<bool> AuthorHasStoryWithTitleAsync(long authorProfileId, string title, CancellationToken cancellationToken = default);

    Task<bool> HasExactlyOnePrimaryGenreAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>Discovery listing: excludes <see cref="StoryStatus.Draft"/> stories. Returns the page plus the total count.</summary>
    Task<(IReadOnlyList<Story> Items, int TotalCount)> SearchPublishedAsync(
        StorySearchCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin/author listing: every status (Draft included), optional
    /// status/genre/keyword filter. Returns the page plus the total count.
    /// </summary>
    Task<(IReadOnlyList<Story> Items, int TotalCount)> SearchAllAsync(
        StorySearchCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>Story count per <see cref="StoryStatus"/> across the whole catalog (admin dashboard).</summary>
    Task<IReadOnlyDictionary<StoryStatus, int>> CountByStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Hydrates entities for a set of ids returned by <see cref="IStorySearchService"/>,
    /// preserving the given order (Elasticsearch's relevance ranking) rather than
    /// re-sorting by any Postgres column. Unknown ids are omitted.
    /// </summary>
    Task<IReadOnlyList<Story>> GetByIdsInOrderAsync(
        IReadOnlyList<long> storyIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every non-Draft story, paged, for the one-time/backfill reindex into
    /// Elasticsearch. Unlike <see cref="SearchPublishedAsync"/> this ignores all
    /// filters/sort — callers page through the whole catalog in id order.
    /// </summary>
    Task<IReadOnlyList<Story>> GetAllPublicPagedAsync(
        long afterId, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ids of stories that changed in the given window — either the
    /// <see cref="Story"/> row itself (title/description/status/etc.) or one of
    /// its <see cref="Chapter"/>s (content edited/approved/removed), matched via
    /// <c>COALESCE(UpdatedAt, CreatedAt)</c> so a never-updated row still counts
    /// by its creation time. Watching Chapters too matters because editing a
    /// chapter's text does not touch the parent Story row. Used by the
    /// background search-index sync job (decoupled from write-path handlers —
    /// see StorySearchIndexSyncCommandHandler); capped at
    /// <paramref name="maxResults"/> distinct story ids.
    /// </summary>
    Task<IReadOnlyList<long>> GetStoryIdsChangedBetweenAsync(
        DateTime sinceExclusive, DateTime untilInclusive, int maxResults, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically moves a story from <see cref="StoryStatus.Draft"/> to
    /// <see cref="StoryStatus.Ongoing"/> and stamps <c>PublishedAt</c> (only if
    /// not already set) when its first chapter is published. A no-op — matching
    /// no row — for a story that is already past Draft, so it is safe to call
    /// from concurrent publishers. Mirrors the manual approve flow's
    /// Draft-&gt;Ongoing transition.
    /// </summary>
    Task TryStartOngoingOnFirstChapterAsync(
        long storyId, DateTime nowUtc, CancellationToken cancellationToken = default);

    Task AddAsync(Story story, CancellationToken cancellationToken = default);

    void Update(Story story);

    /// <summary>
    /// Hard-deletes the story. Callers must have already verified it is safe to
    /// remove (only <see cref="StoryStatus.Draft"/> stories are ever deleted).
    /// Child chapters/volumes/story_genres/story_tags cascade at the database
    /// level (see StoryConfiguration/ChapterConfiguration/VolumeConfiguration).
    /// </summary>
    void Remove(Story story);

    /// <summary>Atomic <c>view_count = view_count + 1</c> for one story; used on the read path.</summary>
    Task IncrementViewCountAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>Story id -&gt; sum of its chapters' <see cref="Content.Domain.Entities.Chapter.CommentCount"/>. Stories with no comments are omitted.</summary>
    Task<IReadOnlyDictionary<long, int>> GetCommentCountsAsync(
        IEnumerable<long> storyIds, CancellationToken cancellationToken = default);

    /// <summary>Story id -&gt; count of its Published chapters. Stories with none are omitted.</summary>
    Task<IReadOnlyDictionary<long, int>> GetPublishedChapterCountsAsync(
        IEnumerable<long> storyIds, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
