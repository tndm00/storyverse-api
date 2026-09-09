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

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
