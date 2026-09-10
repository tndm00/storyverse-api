namespace Authentication.Application.Queries.UsersDirectory;

/// <summary>
/// Internal batch lookup: display names for a set of account ids. Reachable only
/// through the service-token-protected internal endpoint. Unknown ids are
/// silently omitted from the result.
/// </summary>
public sealed class GetUsersDirectoryQuery : IQuery<IReadOnlyList<UserDirectoryEntryDto>>
{
    public IReadOnlyCollection<long> UserIds { get; init; } = Array.Empty<long>();
}
