namespace Moderation.Application.Interfaces.Http;

/// <summary>
/// Outbound calls to the Content service. The <c>Set*Visibility</c> methods carry
/// a moderation decision that MUST take effect: they throw on any non-success
/// response so <c>ResolveReportCommandHandler</c> can abort before marking the
/// report resolved. The title lookup is best-effort and returns an empty map on
/// failure.
/// </summary>
public interface IContentModerationClient
{
    /// <summary>Hides or unhides a story in the Content service. Must succeed or throw before a resolve is committed.</summary>
    Task SetStoryVisibilityAsync(Guid storyId, bool hidden, string reason, CancellationToken cancellationToken);

    /// <summary>Hides or unhides a chapter in the Content service. Must succeed or throw before a resolve is committed.</summary>
    Task SetChapterVisibilityAsync(Guid chapterId, bool hidden, string reason, CancellationToken cancellationToken);

    /// <summary>Public id -&gt; title. <paramref name="targetType"/> selects the story or chapter endpoint.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetTitlesAsync(
        ModerationTargetType targetType, IEnumerable<Guid> ids, CancellationToken cancellationToken);
}
