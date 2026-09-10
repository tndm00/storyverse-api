namespace Content.Application.Interfaces.Services;

/// <summary>
/// Resolves a story's AuthorProfile id to the author's real user id by calling
/// the Authentication service's internal lookup endpoint. Content stores only
/// the AuthorProfile id; the AuthorProfile-&gt;User mapping lives in the
/// Authentication service. Best-effort: callers treat a null result (not found,
/// or the lookup failed) as "no recipient".
/// </summary>
public interface IAuthorDirectoryClient
{
    /// <summary>
    /// GET <c>{BaseUrl}/v1/auth/internal/author-profiles/{authorProfileId}</c>.
    /// Returns the owning user id, or <c>null</c> when no such profile exists.
    /// May throw on transport/HTTP errors.
    /// </summary>
    Task<long?> GetAuthorUserIdAsync(long authorProfileId, CancellationToken cancellationToken);
}
