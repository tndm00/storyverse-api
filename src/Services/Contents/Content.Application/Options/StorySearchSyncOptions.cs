namespace Content.Application.Options;

/// <summary>
/// Binds the <c>StorySearchSync</c> configuration section: the background loop
/// that scans Postgres for stories/chapters changed since the last run and
/// syncs them into Elasticsearch. Fully decoupled from the write-path command
/// handlers — no handler calls <see cref="Interfaces.IStorySearchService"/>
/// directly; this loop is the only writer.
/// </summary>
public sealed class StorySearchSyncOptions
{
    public const string SectionName = "StorySearchSync";

    /// <summary>Master switch for the sync loop. Defaults to enabled — a no-op elsewhere if Elasticsearch:Enabled is false.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>How often the loop scans for changed stories.</summary>
    public int PollIntervalSeconds { get; init; } = 30;

    /// <summary>
    /// Upper bound on distinct changed story ids processed per tick. If a single
    /// window has more changes than this, the excess is picked up on a later
    /// tick (the cursor still advances to "now" each tick regardless — see
    /// StorySearchIndexSyncCommandHandler — so this is a soft cap sized for this
    /// site's actual change volume, not a strict correctness guarantee at
    /// arbitrary scale).
    /// </summary>
    public int BatchSize { get; init; } = 500;
}
