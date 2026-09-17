namespace Content.Application.Interfaces.Services;

/// <summary>
/// Resolves the caller's identity strictly from the validated JWT, never from a
/// request parameter, per auth-guidelines.md section 3. The AuthorProfile row is
/// owned by the Authentication service; this service trusts the <c>author_id</c>
/// claim to establish content ownership.
/// </summary>
public interface ICurrentAuthorContext
{
    /// <summary>True when the current request carries a validated JWT.</summary>
    bool IsAuthenticated { get; }

    /// <summary>True when the validated token carries an author profile claim.</summary>
    bool IsAuthor { get; }

    /// <summary>Returns the caller's user id from the validated JWT.</summary>
    long GetUserId();

    /// <summary>
    /// Returns the caller's AuthorProfile id from the <c>author_id</c> claim.
    /// Throws <see cref="Be.StoryVerse.Core.Exceptions.ForbiddenException"/> when
    /// the caller is authenticated but has no author profile.
    /// </summary>
    long GetAuthorProfileId();

    /// <summary>
    /// True when the caller's role claims expand to the given business permission
    /// (for example <c>content.moderate</c> for staff who may view any story).
    /// </summary>
    bool HasPermission(string permission);
}
