namespace Authentication.Application.Commands.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserSessionIssuer _sessionIssuer;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUserSessionIssuer sessionIssuer,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _sessionIssuer = sessionIssuer;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(ApplicationLogConstants.LoginAttempt, request.Email);

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Return the same generic error for "user not found" and "wrong password"
        // so callers cannot enumerate registered emails, per auth-guidelines.md
        // section 11 (Brute-force Protection Rules).
        if (user is null || string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning(ApplicationLogConstants.LoginFailedInvalidCredentials, request.Email);
            throw new BadRequestException(ApplicationErrorConstants.InvalidCredentials);
        }

        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning(ApplicationLogConstants.LoginFailedAccountNotActive, user.Id);
            throw new BadRequestException(ApplicationErrorConstants.AccountNotActive);
        }

        var session = await _sessionIssuer.IssueAsync(user, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.LoginSucceeded, user.Id);

        return session;
    }
}
