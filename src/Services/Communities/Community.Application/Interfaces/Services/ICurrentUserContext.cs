namespace Community.Application.Interfaces.Services;

/// <summary>
/// Resolves the caller's identity strictly from the validated JWT, never from a
/// request parameter, per auth-guidelines.md section 3. Mirrors the Content
/// service's <c>ICurrentAuthorContext</c> but only needs the user id (<c>sub</c>).
/// </summary>
public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    /// <summary>
    /// Returns the caller's user id from the <c>sub</c> claim. Throws
    /// <see cref="Be.StoryVerse.Core.Exceptions.ForbiddenException"/> when the
    /// request carries no usable subject claim.
    /// </summary>
    long GetUserId();
}
