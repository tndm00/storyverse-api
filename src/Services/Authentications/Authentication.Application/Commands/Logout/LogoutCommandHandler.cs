namespace Authentication.Application.Commands.Logout;

public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LogoutCommandHandler> _logger;

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

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

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
