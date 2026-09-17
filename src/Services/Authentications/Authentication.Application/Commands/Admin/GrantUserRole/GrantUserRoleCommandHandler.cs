namespace Authentication.Application.Commands.Admin.GrantUserRole;

/// <summary>Handles <see cref="GrantUserRoleCommand"/>: validates the role name and target account, then grants the role.</summary>
public sealed class GrantUserRoleCommandHandler : ICommandHandler<GrantUserRoleCommand, UserRolesResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GrantUserRoleCommandHandler> _logger;

    /// <summary>Initializes the handler with the user repository and logger it depends on.</summary>
    public GrantUserRoleCommandHandler(
        IUserRepository userRepository,
        ILogger<GrantUserRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>Grants the requested platform role to the target account and returns its updated role set.</summary>
    public async Task<UserRolesResponseDto> Handle(GrantUserRoleCommand request, CancellationToken cancellationToken)
    {
        // Reject unknown/malformed role names.
        if (!RoleNameParser.TryParse(request.Role, out var role))
        {
            throw new BadRequestException(ApplicationErrorConstants.InvalidRoleName);
        }

        // Ensure the target account exists.
        _ = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.UserNotFound);

        // Grant the role and persist it.
        await _userRepository.GrantRoleAsync(request.UserId, role, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var roles = await _userRepository.GetRolesAsync(request.UserId, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.UserRoleGranted, role, request.UserId);

        return new UserRolesResponseDto
        {
            UserId = request.UserId,
            Roles = roles.Select(r => r.ToString()).ToArray()
        };
    }
}
