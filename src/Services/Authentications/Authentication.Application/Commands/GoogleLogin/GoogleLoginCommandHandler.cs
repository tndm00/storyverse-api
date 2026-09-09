namespace Authentication.Application.Commands.GoogleLogin;

public sealed class GoogleLoginCommandHandler : ICommandHandler<GoogleLoginCommand, LoginResponseDto>
{
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionIssuer _sessionIssuer;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;

    public GoogleLoginCommandHandler(
        IGoogleTokenValidator googleTokenValidator,
        IUserRepository userRepository,
        IUserSessionIssuer sessionIssuer,
        ILogger<GoogleLoginCommandHandler> logger)
    {
        _googleTokenValidator = googleTokenValidator;
        _userRepository = userRepository;
        _sessionIssuer = sessionIssuer;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(ApplicationLogConstants.GoogleLoginAttempt);

        var googleUser = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken);

        if (!googleUser.EmailVerified || string.IsNullOrWhiteSpace(googleUser.Email))
        {
            _logger.LogWarning(ApplicationLogConstants.GoogleLoginFailedUnverifiedEmail);
            throw new BadRequestException(ApplicationErrorConstants.GoogleAuthFailed);
        }

        var user = await ResolveOrProvisionUserAsync(googleUser, cancellationToken);

        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning(ApplicationLogConstants.LoginFailedAccountNotActive, user.Id);
            throw new BadRequestException(ApplicationErrorConstants.AccountNotActive);
        }

        var session = await _sessionIssuer.IssueAsync(user, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.GoogleLoginSucceeded, user.Id);

        return session;
    }

    private async Task<User> ResolveOrProvisionUserAsync(GoogleUserInfo googleUser, CancellationToken cancellationToken)
    {
        // Match on the stable Google subject first so a re-used email address can
        // never resolve to another person's account.
        var user = await _userRepository.GetByExternalIdAsync(
            ApplicationConstants.GoogleProvider, googleUser.Subject, cancellationToken);

        if (user is not null)
        {
            return user;
        }

        user = await _userRepository.GetByEmailAsync(googleUser.Email, cancellationToken);

        if (user is not null)
        {
            // Existing password account with the same verified email: link Google to it.
            user.ExternalProvider = ApplicationConstants.GoogleProvider;
            user.ExternalId = googleUser.Subject;

            if (string.IsNullOrWhiteSpace(user.AvatarUrl))
            {
                user.AvatarUrl = googleUser.PictureUrl;
            }

            // Tracked by the lookup above; the session issuer's SaveChanges persists this.
            _logger.LogInformation(ApplicationLogConstants.GoogleAccountLinked, user.Id);
            return user;
        }

        var provisioned = new User
        {
            Email = googleUser.Email,
            PasswordHash = null,
            DisplayName = string.IsNullOrWhiteSpace(googleUser.Name) ? googleUser.Email : googleUser.Name,
            AvatarUrl = googleUser.PictureUrl,
            Status = UserStatus.Active,
            ExternalProvider = ApplicationConstants.GoogleProvider,
            ExternalId = googleUser.Subject
        };

        provisioned.Roles.Add(new UserRole { Role = Role.Reader });

        await _userRepository.AddAsync(provisioned, cancellationToken);
        _logger.LogInformation(ApplicationLogConstants.GoogleUserProvisioned, googleUser.Email);
        return provisioned;
    }
}
