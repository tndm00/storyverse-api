namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="AuthorProfile"/>, the publishing identity
/// of a <see cref="User"/> (0..1 relationship). Implemented by
/// Authentication.Infrastructure, per code-standard.md section 28.
/// </summary>
public interface IAuthorProfileRepository
{
    Task<AuthorProfile> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    Task<AuthorProfile> GetByIdAsync(long authorProfileId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByUserIdAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Author profiles matching <paramref name="keyword"/> (pen name, case-insensitive
    /// contains) if given, newest first. Used by the platform-admin author roster.
    /// </summary>
    Task<(IReadOnlyList<AuthorProfile> Items, int TotalCount)> GetPagedAsync(
        string keyword,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(AuthorProfile authorProfile, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
