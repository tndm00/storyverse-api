namespace Content.Application.Options;

/// <summary>
/// Binds the <c>ChapterPublishing</c> configuration section: the background loop
/// that turns <see cref="Content.Domain.Enums.ChapterStatus.Scheduled"/> chapters
/// into <see cref="Content.Domain.Enums.ChapterStatus.Published"/> once their
/// <c>ScheduledAt</c> time has passed (product-workflow-context.md section 5.3).
/// </summary>
public sealed class ChapterPublishingOptions
{
    public const string SectionName = "ChapterPublishing";

    /// <summary>Master switch for the auto-publish loop. Defaults to enabled.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>How often the loop scans for due chapters.</summary>
    public int PollIntervalSeconds { get; init; } = 60;

    /// <summary>Upper bound on chapters published per scan, so one pass cannot run unbounded.</summary>
    public int BatchSize { get; init; } = 100;
}
