namespace Notification.Application.Interfaces.Persistence;

/// <summary>
/// Runs a multi-step write as one all-or-nothing database transaction, per
/// code-standard.md section 31. Implemented by Notification.Infrastructure over
/// the scoped <c>NotificationDbContext</c>, so every repository call inside
/// <paramref name="operation"/> shares the transaction.
/// </summary>
public interface INotificationUnitOfWork
{
    /// <summary>Runs <paramref name="operation"/> inside a single database transaction.</summary>
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
