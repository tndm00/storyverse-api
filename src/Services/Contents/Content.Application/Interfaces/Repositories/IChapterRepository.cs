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

    Task<IReadOnlyList<Chapter>> GetByStoryAsync(
        long storyId,
        bool publishedOnly,
        CancellationToken cancellationToken = default);

    Task<bool> StoryHasPublishedChapterAsync(long storyId, CancellationToken cancellationToken = default);

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
