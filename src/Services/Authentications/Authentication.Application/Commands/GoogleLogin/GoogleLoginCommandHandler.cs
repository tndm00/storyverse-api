namespace Authentication.Application.Commands.GoogleLogin;

/// <summary>Handles <see cref="GoogleLoginCommand"/>: validates the Google ID token, resolves or provisions the account, and issues a session.</summary>
public sealed class GoogleLoginCommandHandler : ICommandHandler<GoogleLoginCommand, LoginResponseDto>
{
    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly IUserRepository _userRepository;
    private readonly IUserSessionIssuer _sessionIssuer;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;

    /// <summary>Initializes the handler with the Google token validator, user repository, session issuer, and logger it depends on.</summary>
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

    /// <summary>Validates the Google ID token, resolves or provisions the matching account, and issues an access + refresh token pair.</summary>
    public async Task<LoginResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(ApplicationLogConstants.GoogleLoginAttempt);

        // Verify the ID token with Google and require a verified email.
        var googleUser = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken);

        if (!googleUser.EmailVerified || string.IsNullOrWhiteSpace(googleUser.Email))
        {
            _logger.LogWarning(ApplicationLogConstants.GoogleLoginFailedUnverifiedEmail);
            throw new BadRequestException(ApplicationErrorConstants.GoogleAuthFailed);
        }

        // Match to an existing account (by Google subject or email) or provision a new one.
        var user = await ResolveOrProvisionUserAsync(googleUser, cancellationToken);

        if (user.Status != UserStatus.Active)
        {
            _logger.LogWarning(ApplicationLogConstants.LoginFailedAccountNotActive, user.Id);
            throw new BadRequestException(ApplicationErrorConstants.AccountNotActive);
        }

        // Issue the session token pair.
        var session = await _sessionIssuer.IssueAsync(user, cancellationToken: cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.GoogleLoginSucceeded, user.Id);

        return session;
    }

    /// <summary>Resolves the account matching the Google identity by subject, then email, or provisions a new password-less account.</summary>
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
