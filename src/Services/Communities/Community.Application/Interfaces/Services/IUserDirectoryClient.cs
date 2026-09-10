namespace Community.Application.Interfaces.Services;

/// <summary>
/// Resolves account ids to public display names via the Authentication service's
/// internal batch endpoint. Best-effort by contract: a lookup failure returns
/// the ids it could resolve (possibly none) and never throws, so comment/rating
/// listings still render with the numeric id fallback. Implemented as a
/// request-scoped client that caches within the request, so many comments by the
/// same author cost one lookup.
/// </summary>
public interface IUserDirectoryClient
{
    Task<IReadOnlyDictionary<long, string>> GetDisplayNamesAsync(
        IEnumerable<long> userIds, CancellationToken cancellationToken);
}
