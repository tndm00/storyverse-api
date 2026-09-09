namespace Library.Application.Interfaces.Persistence;

/// <summary>
/// Runs a multi-step write as one all-or-nothing database transaction, per
/// code-standard.md section 31. Implemented by Library.Infrastructure over the
/// scoped <c>LibraryDbContext</c>, so every repository call inside
/// <paramref name="operation"/> shares the transaction.
/// </summary>
public interface ILibraryUnitOfWork
{
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
