namespace Authentication.Application.Commands.Admin.GrantUserRole;

public sealed class GrantUserRoleCommandHandler : ICommandHandler<GrantUserRoleCommand, UserRolesResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GrantUserRoleCommandHandler> _logger;

    public GrantUserRoleCommandHandler(
        IUserRepository userRepository,
        ILogger<GrantUserRoleCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<UserRolesResponseDto> Handle(GrantUserRoleCommand request, CancellationToken cancellationToken)
    {
        if (!RoleNameParser.TryParse(request.Role, out var role))
        {
            throw new BadRequestException(ApplicationErrorConstants.InvalidRoleName);
        }

        _ = await _userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.UserNotFound);

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
