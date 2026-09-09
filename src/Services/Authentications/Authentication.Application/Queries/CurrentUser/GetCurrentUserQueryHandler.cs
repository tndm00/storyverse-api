namespace Authentication.Application.Queries.CurrentUser;

public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, CurrentUserResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(IUserRepository userRepository, ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CurrentUserResponseDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetUserId();

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.UserNotFound);

        var roles = await _userRepository.GetRolesAsync(userId, cancellationToken);

        return user.Adapt<CurrentUserResponseDto>() with
        {
            Roles = roles.Select(role => role.ToString()).ToArray()
        };
    }
}
