namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Resolves the authenticated identity from trusted server-side context (JWT
/// claims), never from request body or query parameters, per
/// auth-guidelines.md section 3 (Identity Source Rules).
/// </summary>
public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    /// <summary>
    /// The authenticated user's internal id, resolved from the <c>sub</c> claim.
    /// Throws if the caller is not authenticated; callers must check
    /// <see cref="IsAuthenticated"/> first or rely on <c>[Authorize]</c> having
    /// already rejected anonymous requests.
    /// </summary>
    long GetUserId();
}
