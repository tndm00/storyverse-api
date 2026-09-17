namespace Library.Application.Interfaces.Services;

/// <summary>
/// Resolves the caller's identity strictly from the validated JWT, never from a
/// request parameter or body, per auth-guidelines.md section 3. The User row is
/// owned by the Authentication service; this service trusts the <c>sub</c> claim
/// to scope every library and reading-progress row.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>Whether the current request carries a validated, authenticated caller.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Returns the caller's user id from the <c>sub</c> claim. Throws
    /// <see cref="Be.StoryVerse.Core.Exceptions.ForbiddenException"/> when the
    /// request carries no usable subject claim.
    /// </summary>
    long GetUserId();
}
