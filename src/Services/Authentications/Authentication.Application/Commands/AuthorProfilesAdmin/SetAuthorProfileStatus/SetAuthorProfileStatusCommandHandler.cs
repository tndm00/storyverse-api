namespace Authentication.Application.Commands.AuthorProfilesAdmin.SetAuthorProfileStatus;

/// <summary>Handles <see cref="SetAuthorProfileStatusCommand"/>: transitions an author profile's lifecycle status.</summary>
public sealed class SetAuthorProfileStatusCommandHandler
    : ICommandHandler<SetAuthorProfileStatusCommand, AdminAuthorProfileResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SetAuthorProfileStatusCommandHandler> _logger;

    /// <summary>Initializes the handler with the repositories and logger it depends on.</summary>
    public SetAuthorProfileStatusCommandHandler(
        IAuthorProfileRepository authorProfileRepository,
        IUserRepository userRepository,
        ILogger<SetAuthorProfileStatusCommandHandler> logger)
    {
        _authorProfileRepository = authorProfileRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>Validates the target status, applies it to the profile, and returns the updated admin view.</summary>
    public async Task<AdminAuthorProfileResponseDto> Handle(
        SetAuthorProfileStatusCommand request,
        CancellationToken cancellationToken)
    {
        // Reject unknown/malformed status names.
        if (!Enum.TryParse<AuthorProfileStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new BadRequestException(ApplicationErrorConstants.InvalidAuthorProfileStatus);
        }

        // Ensure the target profile exists.
        var profile = await _authorProfileRepository.GetByIdAsync(request.AuthorProfileId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.AuthorProfileNotFound);

        // Apply and persist the new status.
        profile.Status = status;

        await _authorProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.AuthorProfileStatusChanged, profile.Id, status);

        // Load the owning account for the response projection.
        var user = await _userRepository.GetByIdAsync(profile.UserId, cancellationToken);

        return new AdminAuthorProfileResponseDto
        {
            AuthorProfileId = profile.Id,
            UserId = profile.UserId,
            Email = user?.Email ?? string.Empty,
            DisplayName = user?.DisplayName ?? string.Empty,
            PenName = profile.PenName,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            BannerUrl = profile.BannerUrl,
            Verified = profile.Verified,
            Status = profile.Status.ToString(),
            CreatedAt = profile.CreatedAt
        };
    }
}
