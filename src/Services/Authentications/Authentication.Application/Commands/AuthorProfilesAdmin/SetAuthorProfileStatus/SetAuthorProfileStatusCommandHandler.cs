namespace Authentication.Application.Commands.AuthorProfilesAdmin.SetAuthorProfileStatus;

public sealed class SetAuthorProfileStatusCommandHandler
    : ICommandHandler<SetAuthorProfileStatusCommand, AdminAuthorProfileResponseDto>
{
    private readonly IAuthorProfileRepository _authorProfileRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SetAuthorProfileStatusCommandHandler> _logger;

    public SetAuthorProfileStatusCommandHandler(
        IAuthorProfileRepository authorProfileRepository,
        IUserRepository userRepository,
        ILogger<SetAuthorProfileStatusCommandHandler> logger)
    {
        _authorProfileRepository = authorProfileRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<AdminAuthorProfileResponseDto> Handle(
        SetAuthorProfileStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AuthorProfileStatus>(request.Status, ignoreCase: true, out var status))
        {
            throw new BadRequestException(ApplicationErrorConstants.InvalidAuthorProfileStatus);
        }

        var profile = await _authorProfileRepository.GetByIdAsync(request.AuthorProfileId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.AuthorProfileNotFound);

        profile.Status = status;

        await _authorProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.AuthorProfileStatusChanged, profile.Id, status);

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
