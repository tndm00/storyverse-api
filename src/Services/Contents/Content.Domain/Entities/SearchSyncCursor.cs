namespace Content.Domain.Entities;

/// <summary>
/// Single-row table holding the high-water mark for the background
/// Elasticsearch search-index sync job (see
/// Content.Application.Commands.Stories.SyncStorySearchIndex). Not a
/// business entity — pure infrastructure state, kept as a table (rather than
/// in-memory) so the sync job survives a service restart without re-scanning
/// every story from the beginning.
/// </summary>
public sealed class SearchSyncCursor : BaseEntity
{
    /// <summary>
    /// Everything with <c>COALESCE(UpdatedAt, CreatedAt)</c> at or before this
    /// timestamp has already been synced to Elasticsearch (or the sync job
    /// determined it wasn't Draft-excluded). The next run only looks after this.
    /// </summary>
    public DateTime LastSyncedAt { get; set; }
}
