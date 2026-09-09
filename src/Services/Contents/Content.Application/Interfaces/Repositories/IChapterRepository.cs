namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="Chapter"/>.
/// </summary>
public interface IChapterRepository
{
    Task<Chapter> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

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
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Highest order index among a story's non-removed chapters, or null when it has none.</summary>
    Task<decimal?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default);

    Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default);

    void Update(Chapter chapter);

    /// <summary>Atomic counter bump for a chapter read; also bumps the parent story's view count.</summary>
    Task IncrementViewCountAsync(long chapterId, long storyId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
