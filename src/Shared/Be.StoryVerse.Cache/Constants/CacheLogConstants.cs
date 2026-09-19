namespace Be.StoryVerse.Cache.Constants;

/// <summary>
/// Centralized structured-logging message templates for the cache project,
/// per code-standard.md section 12 (Logging Rules).
/// </summary>
public static class CacheLogConstants
{
    public const string ViewRecordFallback = "Redis view tracking failed for {Target} {TargetId}; falling back to a direct database increment.";

    public const string ViewStatsReadFailed = "Failed to read view statistics from Redis; daily figures are unavailable.";
}
