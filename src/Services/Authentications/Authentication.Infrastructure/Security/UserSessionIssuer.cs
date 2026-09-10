namespace Authentication.Infrastructure.Security;

/// <summary>
/// Shared tail of every sign-in path: resolves the caller's publishing identity
/// (if any) into the <c>author_id</c> claim, mints the token pair, persists the
/// refresh token (hashed), stamps <see cref="User.LastLoginAt"/>, and — on
/// rotation — revokes the token being replaced, all in one save.
/// </summary>
public sealed class UserSessionIssuer : IUserSessionIssuer
{
    private readonly ITokenService _tokenService;
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public UserSessionIssuer(
        ITokenService tokenService,
        IAuthorProfileRepository authorProfileRepository,
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _tokenService = tokenService;
        _authorProfileRepository = authorProfileRepository;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LoginResponseDto> IssueAsync(
        User user,
        RefreshToken replacedToken = null,
        CancellationToken cancellationToken = default)
    {
        var authorProfile = user.Id > 0
            ? await _authorProfileRepository.GetByUserIdAsync(user.Id, cancellationToken)
            : null;

        // A brand-new account (Google first sign-in) is not persisted yet, so it
        // has no grant rows; GetRolesAsync falls back to Reader for that case.
        // Roles are always read here, never carried over from a previous token,
        // so a role granted since the last sign-in is reflected immediately.
        var roles = user.Id > 0
            ? await _userRepository.GetRolesAsync(user.Id, cancellationToken)
            : new List<Role> { Role.Reader };

        var accessToken = _tokenService.GenerateAccessToken(user, authorProfile?.Id, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Every sign-in path hands us a tracked User (a new Google account is
        // tracked as Added, an existing one from a tracking query). Save once so
        // the user id is assigned before the refresh token row references it.
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);

        var newTokenHash = _tokenService.HashRefreshToken(refreshToken.Value);

        await _refreshTokenRepository.AddAsync(
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newTokenHash,
                ExpiresAt = refreshToken.ExpiresAt.UtcDateTime
            },
            cancellationToken);

        // Rotation: revoke the presented token and link it to its replacement in
        // the same save so the swap is all-or-nothing.
        if (replacedToken is not null)
        {
            replacedToken.RevokedAt = DateTime.UtcNow;
            replacedToken.ReplacedByTokenHash = newTokenHash;
            _refreshTokenRepository.Update(replacedToken);
        }

        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken.Value,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshToken = refreshToken.Value,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt
        };
    }
}
