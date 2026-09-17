namespace Library.Infrastructure.Persistence;

/// <summary>
/// Transaction wrapper over the scoped <see cref="LibraryDbContext"/>. Uses the
/// provider execution strategy so it stays correct if connection retry is ever
/// enabled; the transaction rolls back automatically when <c>operation</c>
/// throws (disposed without a commit).
/// </summary>
public sealed class LibraryUnitOfWork : ILibraryUnitOfWork
{
    private readonly LibraryDbContext _dbContext;

    public LibraryUnitOfWork(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Runs <paramref name="operation"/> inside a database transaction, committing only on success.</summary>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        // Wrap in the provider's execution strategy so retries (if ever enabled) replay the whole transaction.
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Transaction rolls back automatically (on dispose) if operation throws.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
