namespace Content.Infrastructure.Search;

/// <summary>
/// Binds <c>Elasticsearch</c> configuration. <see cref="Enabled"/> is the
/// kill switch for the whole feature: false makes
/// <see cref="ElasticsearchStorySearchService"/> a no-op and
/// GetStoriesQueryHandler falls back to the Postgres ILIKE path, so this can
/// be flipped off with only an env var change + restart, no code rollback.
/// <see cref="SearchReadEnabled"/> lets writes go to Elasticsearch (building
/// up the index) before the read path is cut over to it — see the rollout
/// phasing in the Elasticsearch/Kibana plan.
/// </summary>
public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public string Url { get; init; } = string.Empty;

    public string IndexName { get; init; } = "storyverse-stories";

    /// <summary>Master switch: false disables both dual-write and search reads.</summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// When true (and <see cref="Enabled"/> is true), GetStoriesQueryHandler
    /// routes keyword search through Elasticsearch. Kept separate from
    /// <see cref="Enabled"/> so dual-write can run for a while, backfilling the
    /// index, before the read path is cut over.
    /// </summary>
    public bool SearchReadEnabled { get; init; }
}
