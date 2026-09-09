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

    Task AddAsync(AuthorProfile authorProfile, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
