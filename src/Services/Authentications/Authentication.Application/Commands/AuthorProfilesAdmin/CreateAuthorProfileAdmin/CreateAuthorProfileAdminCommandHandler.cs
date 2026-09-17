namespace Authentication.Application.Commands.AuthorProfilesAdmin.CreateAuthorProfileAdmin;

/// <summary>Handles <see cref="CreateAuthorProfileAdminCommand"/>: creates a new account plus its author profile in a single admin action.</summary>
public sealed class CreateAuthorProfileAdminCommandHandler
    : ICommandHandler<CreateAuthorProfileAdminCommand, AdminAuthorProfileResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateAuthorProfileAdminCommandHandler> _logger;

    /// <summary>Initializes the handler with the repositories, password hasher, and logger it depends on.</summary>
    public CreateAuthorProfileAdminCommandHandler(
        IUserRepository userRepository,
        IAuthorProfileRepository authorProfileRepository,
        IPasswordHasher passwordHasher,
        ILogger<CreateAuthorProfileAdminCommandHandler> logger)
    {
        _userRepository = userRepository;
        _authorProfileRepository = authorProfileRepository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>Creates the account (with Reader + Author roles) and its author profile, then returns the combined admin view.</summary>
    public async Task<AdminAuthorProfileResponseDto> Handle(
        CreateAuthorProfileAdminCommand request,
        CancellationToken cancellationToken)
    {
        // Reject duplicate emails.
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new ConflictException(ApplicationErrorConstants.EmailAlreadyRegistered);
        }

        // Create the account with hashed password and default Reader + Author roles.
        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName
        };
        user.Roles.Add(new UserRole { Role = Role.Reader });
        user.Roles.Add(new UserRole { Role = Role.Author });

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // Create the associated author profile, active by default.
        var authorProfile = new AuthorProfile
        {
            UserId = user.Id,
            PenName = request.PenName.Trim(),
            Bio = request.Bio,
            Status = AuthorProfileStatus.Active
        };

        await _authorProfileRepository.AddAsync(authorProfile, cancellationToken);
        await _authorProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.AuthorProfileCreated, authorProfile.Id, user.Id);

        return new AdminAuthorProfileResponseDto
        {
            AuthorProfileId = authorProfile.Id,
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PenName = authorProfile.PenName,
            Bio = authorProfile.Bio,
            AvatarUrl = authorProfile.AvatarUrl,
            BannerUrl = authorProfile.BannerUrl,
            Verified = authorProfile.Verified,
            Status = authorProfile.Status.ToString(),
            CreatedAt = authorProfile.CreatedAt
        };
    }
}
