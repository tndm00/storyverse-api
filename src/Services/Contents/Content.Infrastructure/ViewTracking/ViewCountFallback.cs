namespace Content.Infrastructure.ViewTracking;

/// <summary>
/// Counts a view with a direct Postgres increment. Used by the Redis view store whenever Redis is
/// disabled or failing, so a view is never lost.
/// </summary>
public sealed class ViewCountFallback : IViewCountFallback
{
    private readonly IStoryRepository _storyRepository;
    private readonly IChapterRepository _chapterRepository;

    public ViewCountFallback(IStoryRepository storyRepository, IChapterRepository chapterRepository)
    {
        _storyRepository = storyRepository;
        _chapterRepository = chapterRepository;
    }

    /// <summary>Adds one view to the story's stored counter.</summary>
    public Task IncrementStoryAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _storyRepository.IncrementViewCountAsync(storyId, cancellationToken);
    }

    /// <summary>Adds one view to the chapter's stored counter (and to its parent story).</summary>
    public Task IncrementChapterAsync(long chapterId, long storyId, CancellationToken cancellationToken = default)
    {
        return _chapterRepository.IncrementViewCountAsync(chapterId, storyId, cancellationToken);
    }
}
