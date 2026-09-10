namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Issues a login session for an already-authenticated <see cref="User"/>:
/// stamps the publishing identity into the access token when the user is an
/// author, mints the refresh token, records the login timestamp, and persists.
/// Shared by every sign-in path (password, Google) so token shaping stays in
/// one place.
/// </summary>
public interface IUserSessionIssuer
{
    /// <summary>
    /// Mints an access + refresh token pair for <paramref name="user"/>,
    /// persists the new refresh token (hashed), stamps the login timestamp, and
    /// saves. When <paramref name="replacedToken"/> is supplied (refresh-token
    /// rotation) it is revoked and linked to the new token in the same save so
    /// the swap is atomic. Roles and the <c>author_id</c> claim are always read
    /// fresh from the database, never copied from a prior token.
    /// </summary>
    Task<LoginResponseDto> IssueAsync(
        User user,
        RefreshToken replacedToken = null,
        CancellationToken cancellationToken = default);
}
