namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="User"/>. Implemented by
/// Authentication.Infrastructure, per code-standard.md section 28.
/// </summary>
public interface IUserRepository
{
    Task<User> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User> GetByExternalIdAsync(string provider, string externalId, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    void Update(User user);

    /// <summary>
    /// Roles granted to the user. Never empty — an account with no grant rows is
    /// still a <see cref="Authentication.Domain.Enums.Role.Reader"/>.
    /// </summary>
    Task<IReadOnlyList<Role>> GetRolesAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Grants <paramref name="role"/> to the user if not already granted. The
    /// caller persists via <see cref="SaveChangesAsync"/>.
    /// </summary>
    Task GrantRoleAsync(long userId, Role role, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
