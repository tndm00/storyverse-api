namespace Community.Infrastructure.Persistence;

/// <summary>
/// Transaction wrapper over the scoped <see cref="CommunityDbContext"/>. Uses the
/// provider execution strategy so it stays correct if connection retry is ever
/// enabled; the transaction rolls back automatically when <c>operation</c> throws
/// (disposed without a commit).
/// </summary>
public sealed class CommunityUnitOfWork : ICommunityUnitOfWork
{
    private readonly CommunityDbContext _dbContext;

    /// <summary>Creates the unit of work over the scoped <see cref="CommunityDbContext"/>.</summary>
    public CommunityUnitOfWork(CommunityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Runs <paramref name="operation"/> inside a database transaction, committing on success and rolling back if it throws.</summary>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        // Use the provider's execution strategy so this stays correct if connection retry is enabled.
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
