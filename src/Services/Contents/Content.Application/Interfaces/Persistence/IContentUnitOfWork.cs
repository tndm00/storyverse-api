namespace Content.Application.Interfaces.Persistence;

/// <summary>
/// Runs a multi-step write as one all-or-nothing database transaction, per
/// code-standard.md section 31 (transactions for multi-step writes that must
/// succeed or fail together). Implemented by Content.Infrastructure over the
/// scoped <c>ContentDbContext</c>, so every repository call inside
/// <paramref name="operation"/> shares the transaction.
/// </summary>
public interface IContentUnitOfWork
{
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
