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
    Task<LoginResponseDto> IssueAsync(User user, CancellationToken cancellationToken = default);
}
