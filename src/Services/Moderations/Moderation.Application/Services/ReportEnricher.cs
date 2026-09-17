namespace Moderation.Application.Services;

/// <summary>
/// Resolved reporter names and target titles for a set of reports, used to
/// enrich the queue list and detail DTOs. Every field is best-effort: a lookup
/// failure just leaves the map entry missing and the DTO falls back to the
/// id-based format.
/// </summary>
public sealed record ReportEnrichmentData(
    IReadOnlyDictionary<long, string> ReporterNames,
    IReadOnlyDictionary<Guid, string> TargetTitles)
{
    /// <summary>An empty result, used when there is nothing to enrich.</summary>
    public static readonly ReportEnrichmentData Empty = new(
        new Dictionary<long, string>(), new Dictionary<Guid, string>());

    /// <summary>Looks up the resolved display name for a reporter, or <c>null</c> if it wasn't resolved.</summary>
    public string ReporterNameFor(long userId) =>
        ReporterNames.TryGetValue(userId, out var name) ? name : null;

    /// <summary>Looks up the resolved title for a report target, or <c>null</c> if it wasn't resolved.</summary>
    public string TargetTitleFor(Guid targetId) =>
        TargetTitles.TryGetValue(targetId, out var title) ? title : null;
}

/// <summary>Fetches reporter display names and target titles for report DTO enrichment.</summary>
public interface IReportEnricher
{
    /// <summary>Fetches reporter display names and target titles/excerpts for the given reports.</summary>
    Task<ReportEnrichmentData> EnrichAsync(IReadOnlyCollection<Report> reports, CancellationToken cancellationToken);
}

/// <summary>
/// Orchestrates the Authentication, Content and Community lookup clients. Lives
/// in the Application layer (it only composes Application-facing client
/// interfaces); the clients themselves are Infrastructure.
/// </summary>
public sealed class ReportEnricher : IReportEnricher
{
    private readonly IUserDirectoryClient _userDirectory;
    private readonly IContentModerationClient _contentClient;
    private readonly ICommunityModerationClient _communityClient;

    public ReportEnricher(
        IUserDirectoryClient userDirectory,
        IContentModerationClient contentClient,
        ICommunityModerationClient communityClient)
    {
        _userDirectory = userDirectory;
        _contentClient = contentClient;
        _communityClient = communityClient;
    }

    /// <summary>
    /// Fetches reporter display names and target titles/excerpts for the given reports in
    /// parallel, across the Authentication, Content, and Community downstream clients.
    /// </summary>
    public async Task<ReportEnrichmentData> EnrichAsync(
        IReadOnlyCollection<Report> reports, CancellationToken cancellationToken)
    {
        if (reports.Count == 0)
        {
            return ReportEnrichmentData.Empty;
        }

        // Group the distinct ids to look up, by kind of target.
        var reporterIds = reports.Select(r => r.ReporterUserId).Where(id => id > 0).Distinct().ToArray();
        var storyIds = TargetIds(reports, ModerationTargetType.Story);
        var chapterIds = TargetIds(reports, ModerationTargetType.Chapter);
        var commentIds = TargetIds(reports, ModerationTargetType.Comment);

        // Fire all downstream lookups concurrently rather than sequentially.
        var namesTask = _userDirectory.GetDisplayNamesAsync(reporterIds, cancellationToken);
        var storyTask = _contentClient.GetTitlesAsync(ModerationTargetType.Story, storyIds, cancellationToken);
        var chapterTask = _contentClient.GetTitlesAsync(ModerationTargetType.Chapter, chapterIds, cancellationToken);
        var commentTask = _communityClient.GetCommentExcerptsAsync(commentIds, cancellationToken);

        await Task.WhenAll(namesTask, storyTask, chapterTask, commentTask);

        // Merge the per-target-type title/excerpt maps into a single lookup.
        var titles = new Dictionary<Guid, string>();
        foreach (var pair in storyTask.Result) titles[pair.Key] = pair.Value;
        foreach (var pair in chapterTask.Result) titles[pair.Key] = pair.Value;
        foreach (var pair in commentTask.Result) titles[pair.Key] = pair.Value;

        return new ReportEnrichmentData(namesTask.Result, titles);
    }

    /// <summary>Distinct target ids of the given type across a set of reports.</summary>
    private static Guid[] TargetIds(IEnumerable<Report> reports, ModerationTargetType type) =>
        reports.Where(r => r.TargetType == type).Select(r => r.TargetId).Distinct().ToArray();
}
