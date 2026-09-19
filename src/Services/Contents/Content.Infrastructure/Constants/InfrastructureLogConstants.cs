namespace Content.Infrastructure.Constants;

/// <summary>
/// Centralized structured-logging message templates for Infrastructure-layer
/// components, per code-standard.md section 12 (Logging Rules).
/// </summary>
public static class InfrastructureLogConstants
{
    public const string SearchIndexFailed = "Failed to index story {StoryId} into Elasticsearch";

    public const string SearchDeleteFailed = "Failed to delete story {StoryId} from Elasticsearch";

    public const string SearchCountFailed = "Failed to get document count from Elasticsearch";

    public const string ViewRecordFallback = "Redis view tracking failed for {Target} {TargetId}; falling back to a direct Postgres increment.";

    public const string ViewStatsReadFailed = "Failed to read view statistics from Redis; daily figures are unavailable.";
}
