namespace Be.StoryVerse.Cache.ViewTracking;

/// <summary>
/// Where a view is counted when Redis is disabled or failing. Owned by the cache project and
/// implemented by the service that owns the view counters (a direct database increment), so
/// <see cref="RedisViewStore"/> never depends on a service's repositories.
/// </summary>
public interface IViewCountFallback
{
    /// <summary>Adds one view to a story's stored counter.</summary>
    /// <param name="storyId">Internal id of the viewed story.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IncrementStoryAsync(long storyId, CancellationToken cancellationToken = default);

    /// <summary>Adds one view to a chapter's stored counter (and to its parent story).</summary>
    /// <param name="chapterId">Internal id of the read chapter.</param>
    /// <param name="storyId">Internal id of the chapter's parent story.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task IncrementChapterAsync(long chapterId, long storyId, CancellationToken cancellationToken = default);
}
