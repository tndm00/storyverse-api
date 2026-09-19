namespace Content.Application.Interfaces.Services;

/// <summary>One entry of the "most viewed today" ranking.</summary>
public sealed class TopStoryViews
{
    /// <summary>Internal story id.</summary>
    public long StoryId { get; init; }

    /// <summary>Views the story received on the ranked day.</summary>
    public long Views { get; init; }
}

/// <summary>A point-in-time read of the Redis view statistics.</summary>
public sealed class ViewStatsSnapshot
{
    /// <summary>False when Redis is disabled or could not be read; the daily figures are then meaningless.</summary>
    public bool IsAvailable { get; init; }

    /// <summary>Views recorded today (business day).</summary>
    public long TodayViews { get; init; }

    /// <summary>Views recorded yesterday (business day).</summary>
    public long YesterdayViews { get; init; }

    /// <summary>Views still buffered in Redis and not yet added to the Postgres story counters.</summary>
    public long PendingStoryViews { get; init; }

    /// <summary>The first business day the daily counters were kept, or null before the first view is recorded.</summary>
    public DateOnly? TrackingSince { get; init; }

    /// <summary>The most viewed stories today, highest first.</summary>
    public IReadOnlyList<TopStoryViews> TopStoriesToday { get; init; } = Array.Empty<TopStoryViews>();
}

/// <summary>Reads the daily view statistics kept in Redis.</summary>
public interface IViewStatsReader
{
    /// <summary>Reads today's and yesterday's totals, the buffered-but-unflushed views and today's top stories.</summary>
    /// <param name="topCount">How many top stories to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<ViewStatsSnapshot> GetSnapshotAsync(int topCount, CancellationToken cancellationToken = default);
}
