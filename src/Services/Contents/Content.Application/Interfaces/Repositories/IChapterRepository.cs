namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="Chapter"/>.
/// </summary>
public interface IChapterRepository
{
    Task<Chapter> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    /// <summary>Public id -&gt; title for a set of chapters, for internal cross-service lookups. Unknown ids are omitted.</summary>
    Task<IReadOnlyList<ContentTitleEntryDto>> GetTitlesByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Public id -&gt; chapter title plus parent story id/slug/title, for internal
    /// cross-service lookups that need story context (not just the chapter
    /// title). Unknown ids are omitted.
    /// </summary>
    Task<IReadOnlyList<ChapterContextEntryDto>> GetContextByPublicIdsAsync(
        IEnumerable<Guid> publicIds, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Chapter>> GetByStoryAsync(
        long storyId,
        bool publishedOnly,
        CancellationToken cancellationToken = default);

    Task<bool> StoryHasPublishedChapterAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>A volume's non-removed chapters as change-tracked entities, for a bulk reorder.</summary>
    Task<IReadOnlyList<Chapter>> GetByVolumeTrackedAsync(long volumeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// A story's non-removed chapters that belong to no volume, as change-tracked
    /// entities, for a bulk reorder.
    /// </summary>
    Task<IReadOnlyList<Chapter>> GetStoryChaptersWithoutVolumeTrackedAsync(
        long storyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scheduled chapters whose <see cref="Chapter.ScheduledAt"/> is at or before
    /// <paramref name="asOfUtc"/>, oldest schedule first, capped at
    /// <paramref name="maxItems"/>. Read-only — publishing is done via
    /// <see cref="TryMarkPublishedAsync"/>.
    /// </summary>
    Task<IReadOnlyList<Chapter>> GetDueScheduledAsync(
        DateTime asOfUtc, int maxItems, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atomically flips one chapter from <see cref="ChapterStatus.Scheduled"/> to
    /// <see cref="ChapterStatus.Published"/> (setting <c>PublishedAt</c> and
    /// <c>UpdatedAt</c>). Returns <c>true</c> only when this call performed the
    /// transition, so it is safe to run from multiple instances — the loser's
    /// <c>WHERE status = 'Scheduled'</c> guard matches no row.
    /// </summary>
    Task<bool> TryMarkPublishedAsync(
        long chapterId, DateTime nowUtc, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cross-story moderation queue: chapters awaiting a decision (PendingReview
    /// and/or InReview), oldest first, joined to their parent story.
    /// </summary>
    Task<(IReadOnlyList<(Chapter Chapter, Story Story)> Items, int TotalCount)> GetPendingReviewAsync(
        ChapterStatus? status,
        string keyword,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Chapters that have a moderator decision of <paramref name="actionType"/> in
    /// their review audit trail (Approved or Rejected history), newest first.
    /// </summary>
    Task<(IReadOnlyList<(Chapter Chapter, Story Story)> Items, int TotalCount)> GetReviewedAsync(
        ChapterReviewActionType actionType,
        string keyword,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Review dashboard counters: chapters currently PendingReview / InReview, plus
    /// distinct chapters ever Approved / Rejected (from the audit trail).
    /// </summary>
    Task<(int Pending, int InReview, int Approved, int Rejected)> GetReviewCountsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Highest order index among a story's non-removed chapters, or null when it has none.</summary>
    Task<decimal?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default);

    Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default);

    void Update(Chapter chapter);

    /// <summary>Atomic counter bump for a chapter read; also bumps the parent story's view count.</summary>
    Task IncrementViewCountAsync(long chapterId, long storyId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
