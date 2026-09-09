namespace Authentication.Infrastructure.Security;

/// <summary>
/// Shared tail of every sign-in path: resolves the caller's publishing identity
/// (if any) into the <c>author_id</c> claim, mints the token pair, stamps
/// <see cref="User.LastLoginAt"/>, and persists in one save.
/// </summary>
public sealed class UserSessionIssuer : IUserSessionIssuer
{
    private readonly ITokenService _tokenService;
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IUserRepository _userRepository;

    public UserSessionIssuer(
        ITokenService tokenService,
        IAuthorProfileRepository authorProfileRepository,
        IUserRepository userRepository)
    {
        _tokenService = tokenService;
        _authorProfileRepository = authorProfileRepository;
        _userRepository = userRepository;
    }

    public async Task<LoginResponseDto> IssueAsync(User user, CancellationToken cancellationToken = default)
    {
        var authorProfile = user.Id > 0
            ? await _authorProfileRepository.GetByUserIdAsync(user.Id, cancellationToken)
            : null;

        // A brand-new account (Google first sign-in) is not persisted yet, so it
        // has no grant rows; GetRolesAsync falls back to Reader for that case.
        var roles = user.Id > 0
            ? await _userRepository.GetRolesAsync(user.Id, cancellationToken)
            : new List<Role> { Role.Reader };

        var accessToken = _tokenService.GenerateAccessToken(user, authorProfile?.Id, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Every sign-in path hands us a tracked User (a new Google account is
        // tracked as Added, an existing one from a tracking query), so mutating
        // the timestamp and saving is enough to persist both it and any pending
        // insert/link from the caller.
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = accessToken.Value,
            AccessTokenExpiresAt = accessToken.ExpiresAt,
            RefreshToken = refreshToken.Value,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt
        };
    }
}
