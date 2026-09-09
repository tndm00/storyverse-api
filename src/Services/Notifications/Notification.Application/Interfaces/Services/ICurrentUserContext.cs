namespace Notification.Application.Interfaces.Services;

/// <summary>
/// Resolves the caller's identity strictly from the validated JWT <c>sub</c>
/// claim, never from a request parameter or body, per auth-guidelines.md
/// section 3. Mirrors the Content service's <c>ICurrentAuthorContext</c>.
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
