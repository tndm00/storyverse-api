namespace Content.Infrastructure.Constants;

/// <summary>
/// Centralized structured-logging message templates for Infrastructure-layer
/// components, per code-standard.md section 12 (Logging Rules).
/// </summary>
public static class InfrastructureLogConstants
{
    public const string SearchIndexFailed = "Failed to index story {StoryId} into Elasticsearch";

    public const string SearchDeleteFailed = "Failed to delete story {StoryId} from Elasticsearch";
}
