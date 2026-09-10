namespace Authentication.Application.Commands.Admin.RevokeUserRole;

public sealed class RevokeUserRoleCommandHandler : ICommandHandler<RevokeUserRoleCommand, UserRolesResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<RevokeUserRoleCommandHandler> _logger;

    public RevokeUserRoleCommandHandler(
        IUserRepository userRepository,
        ILogger<RevokeUserRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserRolesResponseDto> Handle(RevokeUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!RoleNameParser.TryParse(request.Role, out var role))
        {
            throw new BadRequestException(ApplicationErrorConstants.InvalidRoleName);
        }

        if (role == Role.Reader)
        {
            throw new BadRequestException(ApplicationErrorConstants.ReaderRoleCannotBeRevoked);
        }

        _ = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.UserNotFound);

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
