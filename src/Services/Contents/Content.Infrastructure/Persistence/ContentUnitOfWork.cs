namespace Content.Infrastructure.Persistence;

/// <summary>
/// Transaction wrapper over the scoped <see cref="ContentDbContext"/>. Uses the
/// provider execution strategy so it stays correct if connection retry is ever
/// enabled; the transaction rolls back automatically when
/// <c>operation</c> throws (disposed without a commit).
/// </summary>
public sealed class ContentUnitOfWork : IContentUnitOfWork
{
    private readonly ContentDbContext _dbContext;

    public ContentUnitOfWork(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Runs <paramref name="operation"/> inside a database transaction, via the provider's
    /// execution strategy. The transaction commits only if the operation completes without
    /// throwing; otherwise it is disposed and rolled back.
    /// </summary>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Begin the transaction, run the caller's operation, then commit.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
