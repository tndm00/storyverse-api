namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="User"/>. Implemented by
/// Authentication.Infrastructure, per code-standard.md section 28.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    void Update(User user);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
