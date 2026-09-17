namespace Authentication.Application.Commands.Admin.RevokeUserRole;

/// <summary>Handles <see cref="RevokeUserRoleCommand"/>: validates the role name and target account, then revokes the role.</summary>
public sealed class RevokeUserRoleCommandHandler : ICommandHandler<RevokeUserRoleCommand, UserRolesResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RevokeUserRoleCommandHandler> _logger;

    /// <summary>Initializes the handler with the user repository and logger it depends on.</summary>
    public RevokeUserRoleCommandHandler(
        IUserRepository userRepository,
        ILogger<RevokeUserRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>Revokes the requested platform role from the target account and returns its updated role set.</summary>
    public async Task<UserRolesResponseDto> Handle(RevokeUserRoleCommand request, CancellationToken cancellationToken)
    {
        // Reject unknown/malformed role names.
        if (!RoleNameParser.TryParse(request.Role, out var role))
        {
            throw new BadRequestException(ApplicationErrorConstants.InvalidRoleName);
        }

        // The implicit Reader floor cannot be revoked.
        if (role == Role.Reader)
        {
            throw new BadRequestException(ApplicationErrorConstants.ReaderRoleCannotBeRevoked);
        }

        // Ensure the target account exists.
        _ = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.UserNotFound);

        // Revoke the role and persist it.
        await _userRepository.RevokeRoleAsync(request.UserId, role, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var roles = await _userRepository.GetRolesAsync(request.UserId, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.UserRoleRevoked, role, request.UserId);

        return new UserRolesResponseDto
        {
            UserId = request.UserId,
            Roles = roles.Select(r => r.ToString()).ToArray()
        };
    }
}
