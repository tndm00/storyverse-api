namespace Authentication.Application.Commands.Logout;

/// <summary>Handles <see cref="LogoutCommand"/>: revokes the caller's refresh token if it is live and owned by them.</summary>
public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutCommandHandler> _logger;

    /// <summary>Initializes the handler with the refresh token repository, token service, current-user accessor, and logger it depends on.</summary>
    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        ICurrentUserService currentUserService,
        ILogger<LogoutCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>Revokes the given refresh token when it is live and owned by the caller; otherwise no-ops.</summary>
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        // Look up the stored token by its hash.
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        // Only revoke a live token that actually belongs to the caller. Anything
        // else is a no-op so logout cannot be used to probe other accounts.
        if (stored is not null && stored.UserId == userId && stored.RevokedAt is null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            _refreshTokenRepository.Update(stored);
            await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(ApplicationLogConstants.LogoutSucceeded, userId);
        }

        return Unit.Value;
    }
}
