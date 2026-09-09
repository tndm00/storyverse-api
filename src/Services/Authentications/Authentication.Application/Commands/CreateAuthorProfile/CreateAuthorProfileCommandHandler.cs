namespace Authentication.Application.Commands.CreateAuthorProfile;

public sealed class CreateAuthorProfileCommandHandler
    : ICommandHandler<CreateAuthorProfileCommand, AuthorProfileResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateAuthorProfileCommandHandler> _logger;

    public CreateAuthorProfileCommandHandler(
        IAuthorProfileRepository authorProfileRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateAuthorProfileCommandHandler> logger)
    {
        _authorProfileRepository = authorProfileRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<AuthorProfileResponseDto> Handle(
        CreateAuthorProfileCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        _logger.LogInformation(ApplicationLogConstants.AuthorProfileCreateAttempt, userId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.UserNotFound);

        if (user.Status != UserStatus.Active)
        {
            throw new BadRequestException(ApplicationErrorConstants.AccountNotActive);
        }

        // A user holds at most one AuthorProfile (0..1), enforced here and by the
        // unique index on user_id in AuthorProfileConfiguration.
        if (await _authorProfileRepository.ExistsByUserIdAsync(userId, cancellationToken))
        {
            _logger.LogWarning(ApplicationLogConstants.AuthorProfileCreateFailedExists, userId);
            throw new ConflictException(ApplicationErrorConstants.AuthorProfileAlreadyExists);
        }

        var authorProfile = new AuthorProfile
        {
            UserId = userId,
            PenName = request.PenName.Trim(),
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl,
            BannerUrl = request.BannerUrl,
            Status = AuthorProfileStatus.Active
        };

        await _authorProfileRepository.AddAsync(authorProfile, cancellationToken);

        // Becoming an author grants the Author role. The new claim reaches the
        // caller on their next sign-in (RequiresTokenRefresh below).
        await _userRepository.GrantRoleAsync(userId, Role.Author, cancellationToken);

        await _authorProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.AuthorProfileCreated, authorProfile.Id, userId);

        return new AuthorProfileResponseDto
        {
            AuthorProfileId = authorProfile.Id,
            UserId = authorProfile.UserId,
            PenName = authorProfile.PenName,
            Bio = authorProfile.Bio,
            AvatarUrl = authorProfile.AvatarUrl,
            BannerUrl = authorProfile.BannerUrl,
            Verified = authorProfile.Verified,
            Status = authorProfile.Status.ToString(),
            CreatedAt = authorProfile.CreatedAt,
            RequiresTokenRefresh = true
        };
    }
}
