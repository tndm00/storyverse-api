namespace Moderation.Application.Interfaces.Http;

/// <summary>
/// Outbound calls to the Community service. <see cref="SetCommentVisibilityAsync"/>
/// carries a moderation decision that MUST take effect and throws on any
/// non-success response. <see cref="GetCommentExcerptsAsync"/> is best-effort.
/// </summary>
public interface ICommunityModerationClient
{
    Task SetCommentVisibilityAsync(Guid commentId, bool hidden, string reason, CancellationToken cancellationToken);

    /// <summary>Comment public id -&gt; short content excerpt, for the reports queue summary.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetCommentExcerptsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken);
}
