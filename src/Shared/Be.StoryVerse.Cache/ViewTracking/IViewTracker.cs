namespace Be.StoryVerse.Cache.ViewTracking;

/// <summary>
/// Records reader views on the read path. Implemented over Redis (a fast in-memory
/// write buffer, flushed to Postgres in batches); when Redis is disabled or fails the
/// implementation falls back to a direct Postgres increment, so a view is never lost
/// and a Redis outage never breaks reading.
/// </summary>
public interface IViewTracker
{
    /// <summary>Counts one view of a story page.</summary>
    /// <param name="storyId">Internal id of the viewed story.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordStoryViewAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>Counts one read of a chapter; this also counts as a view of its parent story.</summary>
    /// <param name="chapterId">Internal id of the read chapter.</param>
    /// <param name="storyId">Internal id of the chapter's parent story.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordChapterViewAsync(long chapterId, long storyId, CancellationToken cancellationToken = default);
}
