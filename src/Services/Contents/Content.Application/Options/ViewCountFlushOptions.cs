namespace Content.Application.Options;

/// <summary>
/// Binds the <c>ViewCountFlush</c> configuration section: the background loop that moves view
/// counts buffered in Redis into the Postgres <c>ViewCount</c> columns in batches, instead of one
/// <c>UPDATE</c> per page view.
/// </summary>
public sealed class ViewCountFlushOptions
{
    public const string SectionName = "ViewCountFlush";

    /// <summary>Master switch for the flush loop. It is also a no-op when Redis itself is disabled.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>How often buffered counts are flushed to Postgres.</summary>
    public int FlushIntervalSeconds { get; init; } = 30;
}
