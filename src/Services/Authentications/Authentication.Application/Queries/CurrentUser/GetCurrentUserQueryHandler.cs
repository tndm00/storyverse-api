namespace Authentication.Application.Queries.CurrentUser;

/// <summary>Handles <see cref="GetCurrentUserQuery"/>.</summary>
public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, CurrentUserResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    /// <summary>Resolves the caller's profile and current role names from the trusted auth context.</summary>
    public async Task<CurrentUserResponseDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Identity comes from the trusted auth context, never from the request.
        var userId = _currentUserService.GetUserId();

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.UserNotFound);

        // Roles are loaded separately and merged into the mapped DTO.
        var roles = await _userRepository.GetRolesAsync(userId, cancellationToken);

        return user.Adapt<CurrentUserResponseDto>() with
        {
            Roles = roles.Select(role => role.ToString()).ToArray()
        };
    }
}
