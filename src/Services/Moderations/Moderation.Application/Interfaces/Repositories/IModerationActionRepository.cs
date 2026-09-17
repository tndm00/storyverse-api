namespace Moderation.Application.Interfaces.Repositories;

/// <summary>
/// Append-only persistence boundary for the <see cref="ModerationAction"/> audit
/// trail. There is deliberately no update or delete member — audit records are
/// immutable (product-workflow-context.md section 4).
/// </summary>
public interface IModerationActionRepository
{
    /// <summary>Returns all moderation actions recorded against a report, for its audit trail.</summary>
    Task<IReadOnlyList<ModerationAction>> GetByReportIdAsync(long reportId, CancellationToken cancellationToken = default);

    /// <summary>Queues a new moderation action for insertion.</summary>
    Task AddAsync(ModerationAction action, CancellationToken cancellationToken = default);

    /// <summary>Persists queued changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
