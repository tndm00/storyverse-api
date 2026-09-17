namespace Authentication.Application.Queries.UsersDirectory;

/// <summary>Handles <see cref="GetUsersDirectoryQuery"/>.</summary>
public sealed class GetUsersDirectoryQueryHandler
    : IQueryHandler<GetUsersDirectoryQuery, IReadOnlyList<UserDirectoryEntryDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersDirectoryQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>Batch-resolves display names for the given account ids; unknown ids are omitted.</summary>
    public async Task<IReadOnlyList<UserDirectoryEntryDto>> Handle(
        GetUsersDirectoryQuery request,
        CancellationToken cancellationToken)
    {
        // Normalize input: drop invalid/duplicate ids, and short-circuit an empty request.
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
