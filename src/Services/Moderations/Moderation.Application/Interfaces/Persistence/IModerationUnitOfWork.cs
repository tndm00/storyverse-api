namespace Moderation.Application.Interfaces.Persistence;

/// <summary>
/// Runs a multi-step write as one all-or-nothing database transaction, per
/// code-standard.md section 31. Used when a report status change and its
/// <see cref="ModerationAction"/> audit row must be committed together.
/// Implemented by Moderation.Infrastructure over the scoped
/// <c>ModerationDbContext</c>.
/// </summary>
public interface IModerationUnitOfWork
{
    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}
