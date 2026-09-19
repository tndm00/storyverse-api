namespace Be.StoryVerse.Cache.ViewTracking;

/// <summary>
/// The view counts buffered in Redis that have not yet been added to the Postgres
/// <c>ViewCount</c> columns. Keys are internal story/chapter ids, values are the number of
/// views waiting to be applied.
/// </summary>
public sealed class PendingViewCounts
{
    /// <summary>Story id -&gt; buffered views.</summary>
    public IReadOnlyDictionary<long, long> Stories { get; init; } = new Dictionary<long, long>();

    /// <summary>Chapter id -&gt; buffered views.</summary>
    public IReadOnlyDictionary<long, long> Chapters { get; init; } = new Dictionary<long, long>();

    /// <summary>True when nothing is waiting to be flushed.</summary>
    public bool IsEmpty => Stories.Count == 0 && Chapters.Count == 0;
}

/// <summary>
/// Hand-off point between the Redis view buffer and the periodic flush to Postgres.
/// Taking a batch does not delete it: it stays parked until <see cref="CommitAsync"/> confirms
/// Postgres has it, so a crash in between re-serves the same batch (at-least-once).
/// </summary>
public interface IViewCountBuffer
{
    /// <summary>
    /// Atomically takes everything buffered so far (or re-serves a previous batch that was never
    /// committed). Returns an empty batch when nothing is pending or Redis is unavailable.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PendingViewCounts> TakePendingAsync(CancellationToken cancellationToken = default);

    /// <summary>Discards the batch returned by the last <see cref="TakePendingAsync"/> once it is safely in Postgres.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CommitAsync(CancellationToken cancellationToken = default);
}
