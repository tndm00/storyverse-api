namespace Authentication.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AuthenticationDbContext _dbContext;

    public UserRepository(AuthenticationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByIdsAsync(
        IEnumerable<long> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.Distinct().ToArray();
        if (idList.Length == 0)
        {
            return Array.Empty<User>();
        }

        return await _dbContext.Users
            .Where(x => idList.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    public Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public Task<User> GetByExternalIdAsync(string provider, string externalId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.FirstOrDefaultAsync(
            x => x.ExternalProvider == provider && x.ExternalId == externalId,
            cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _dbContext.Users.Update(user);
    }

    public async Task<IReadOnlyList<Role>> GetRolesAsync(long userId, CancellationToken cancellationToken = default)
    {
        var roles = await _dbContext.UserRoles
            .Where(x => x.UserId == userId)
            .Select(x => x.Role)
            .ToListAsync(cancellationToken);

        return roles.Count > 0 ? roles : new List<Role> { Role.Reader };
    }

    public async Task GrantRoleAsync(long userId, Role role, CancellationToken cancellationToken = default)
    {
        var alreadyGranted = await _dbContext.UserRoles
            .AnyAsync(x => x.UserId == userId && x.Role == role, cancellationToken);

        if (alreadyGranted)
        {
            return;
        }

        await _dbContext.UserRoles.AddAsync(new UserRole { UserId = userId, Role = role }, cancellationToken);
    }

    public async Task RevokeRoleAsync(long userId, Role role, CancellationToken cancellationToken = default)
    {
        var grant = await _dbContext.UserRoles
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Role == role, cancellationToken);

        if (grant is not null)
        {
            _dbContext.UserRoles.Remove(grant);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
