namespace Authentication.Application.Queries.UsersDirectory;

public sealed class GetUsersDirectoryQueryHandler
    : IQueryHandler<GetUsersDirectoryQuery, IReadOnlyList<UserDirectoryEntryDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersDirectoryQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDirectoryEntryDto>> Handle(
        GetUsersDirectoryQuery request,
        CancellationToken cancellationToken)
    {
        var distinctIds = request.UserIds.Where(id => id > 0).Distinct().ToArray();
        if (distinctIds.Length == 0)
        {
            return Array.Empty<UserDirectoryEntryDto>();
        }

        var users = await _userRepository.GetByIdsAsync(distinctIds, cancellationToken);

        return users
            .Select(u => new UserDirectoryEntryDto { UserId = u.Id, DisplayName = u.DisplayName })
            .ToArray();
    }
}
