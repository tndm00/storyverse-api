namespace Notification.Infrastructure.Persistence;

/// <summary>
/// Transaction wrapper over the scoped <see cref="NotificationDbContext"/>. Uses
/// the provider execution strategy so it stays correct if connection retry is
/// ever enabled; the transaction rolls back automatically when
/// <c>operation</c> throws (disposed without a commit).
/// </summary>
public sealed class NotificationUnitOfWork : INotificationUnitOfWork
{
    private readonly NotificationDbContext _dbContext;

    public NotificationUnitOfWork(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Runs <paramref name="operation"/> inside a single database transaction via the provider's execution strategy.</summary>
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // Begin the transaction, run the caller's operation, then commit; an exception
            // leaves the transaction unconsumed so it rolls back on dispose.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}
