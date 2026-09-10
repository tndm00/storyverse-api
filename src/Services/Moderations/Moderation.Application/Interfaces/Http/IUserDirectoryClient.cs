namespace Moderation.Application.Interfaces.Http;

/// <summary>
/// Resolves reporter account ids to display names via the Authentication
/// service's internal batch endpoint. Best-effort: returns an empty map on
/// failure so the reports queue falls back to the numeric-id format.
/// </summary>
public interface IUserDirectoryClient
{
    Task<IReadOnlyDictionary<long, string>> GetDisplayNamesAsync(
        IEnumerable<long> userIds, CancellationToken cancellationToken);
}
