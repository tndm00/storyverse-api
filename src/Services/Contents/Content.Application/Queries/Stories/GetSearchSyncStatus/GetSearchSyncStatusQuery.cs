namespace Content.Application.Queries.Stories.GetSearchSyncStatus;

/// <summary>
/// Elasticsearch sync status for the admin console: how far the background
/// sync job has gotten and how many documents are actually in the index.
/// Requires <c>content.moderate</c>.
/// </summary>
public sealed class GetSearchSyncStatusQuery : IQuery<SearchSyncStatusResponseDto>
{
}

public sealed class SearchSyncStatusResponseDto
{
    /// <summary>Null when the background sync job has never run yet.</summary>
    public DateTime? LastSyncedAt { get; init; }

    /// <summary>Document count in the Elasticsearch index right now.</summary>
    public long SyncedDocumentCount { get; init; }

    /// <summary>Non-Draft story count in Postgres — the target the index should eventually match.</summary>
    public int EligibleStoryCount { get; init; }

    /// <summary>Mirrors <c>Elasticsearch:Enabled</c> — whether the sync job is actually writing to Elasticsearch.</summary>
    public bool Enabled { get; init; }

    /// <summary>Mirrors <c>Elasticsearch:SearchReadEnabled</c> — whether public search is actually reading from Elasticsearch.</summary>
    public bool SearchReadEnabled { get; init; }
}
