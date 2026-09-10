namespace Authentication.Application.Commands.Refresh;

public sealed class RefreshCommandHandler : ICommandHandler<RefreshCommand, LoginResponseDto>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUserSessionIssuer _sessionIssuer;
    private readonly ILogger<RefreshCommandHandler> _logger;

    public RefreshCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        IUserSessionIssuer sessionIssuer,
        ILogger<RefreshCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _sessionIssuer = sessionIssuer;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(ApplicationLogConstants.RefreshAttempt);

        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (stored is null)
        {
            _logger.LogWarning(ApplicationLogConstants.RefreshFailedUnknownOrExpired);
            throw new UnauthorizedException(ApplicationErrorConstants.InvalidRefreshToken);
        }

        var now = DateTime.UtcNow;

        // A token that was already rotated out (revoked) is being replayed: treat
        // the account as compromised and drop every live token for it, per
        // auth-guidelines.md section 9 (reuse detection).
        if (stored.RevokedAt is not null)
        {
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(stored.UserId, now, cancellationToken);
            _logger.LogWarning(ApplicationLogConstants.RefreshTokenReuseDetected, stored.UserId);
            throw new UnauthorizedException(ApplicationErrorConstants.InvalidRefreshToken);
        }

        if (stored.ExpiresAt <= now)
        {
            _logger.LogWarning(ApplicationLogConstants.RefreshFailedUnknownOrExpired);
            throw new UnauthorizedException(ApplicationErrorConstants.InvalidRefreshToken);
        }

        var user = await _userRepository.GetByIdAsync(stored.UserId, cancellationToken);

        if (user is null || user.Status != UserStatus.Active)
        {
            _logger.LogWarning(ApplicationLogConstants.RefreshFailedUnknownOrExpired);
            throw new UnauthorizedException(ApplicationErrorConstants.InvalidRefreshToken);
        }

        var session = await _sessionIssuer.IssueAsync(user, stored, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.RefreshSucceeded, user.Id);

        return session;
    }
}
