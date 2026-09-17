namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IUserRepository"/>.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly AuthenticationDbContext _dbContext;

    /// <summary>
    /// Creates the repository bound to the given <see cref="AuthenticationDbContext"/>.
    /// </summary>
    public UserRepository(AuthenticationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Looks up a user by id.
    /// </summary>
    public Task<User> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>
    /// Looks up multiple users by id in a single query; returns an empty list if no ids are given.
    /// </summary>
    public async Task<IReadOnlyList<User>> GetByIdsAsync(
        IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        // Short-circuit to avoid issuing a query with an empty IN clause.
        var idList = ids.Distinct().ToArray();
        if (idList.Length == 0)
        {
            return Array.Empty<User>();
        }

        return await _dbContext.Users
            .Where(x => idList.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Looks up a user by email.
    /// </summary>
    public Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    /// <summary>
    /// Looks up a user by the external identity provider and its subject id.
    /// </summary>
    public Task<User> GetByExternalIdAsync(string provider, string externalId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(
            x => x.ExternalProvider == provider && x.ExternalId == externalId,
            cancellationToken);
    }

    /// <summary>
    /// Checks whether a user with the given email already exists.
    /// </summary>
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    /// <summary>
    /// Stages a new user for insertion; persisted on <see cref="SaveChangesAsync"/>.
    /// </summary>
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    /// <summary>
    /// Marks an already-tracked user as modified.
    /// </summary>
    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    /// <summary>
    /// Returns the roles granted to a user, defaulting to <see cref="Role.Reader"/>
    /// when no explicit grants exist.
    /// </summary>
    public async Task<IReadOnlyList<Role>> GetRolesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.UserRoles
            .Where(x => x.UserId == userId)
            .Select(x => x.Role)
            .ToListAsync(cancellationToken);

        return roles.Count > 0 ? roles : new List<Role> { Role.Reader };
    }

    /// <summary>
    /// Grants a role to a user, no-op if the grant already exists.
    /// </summary>
    public async Task GrantRoleAsync(long userId, Role role, CancellationToken cancellationToken = default)
    {
        // Avoid a duplicate grant (composite key would otherwise throw on save).
        var alreadyGranted = await _dbContext.UserRoles
            .AnyAsync(x => x.UserId == userId && x.Role == role, cancellationToken);

        if (alreadyGranted)
        {
            return;
        }

        await _dbContext.UserRoles.AddAsync(new UserRole { UserId = userId, Role = role }, cancellationToken);
    }

    /// <summary>
    /// Revokes a role from a user, no-op if the grant does not exist.
    /// </summary>
    public async Task RevokeRoleAsync(long userId, Role role, CancellationToken cancellationToken = default)
    {
        var grant = await _dbContext.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Role == role, cancellationToken);

        if (grant is not null)
        {
            _dbContext.UserRoles.Remove(grant);
        }
    }

    /// <summary>
    /// Persists all pending changes tracked by the context.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
