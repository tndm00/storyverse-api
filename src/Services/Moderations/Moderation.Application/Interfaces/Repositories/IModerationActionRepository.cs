namespace Moderation.Application.Interfaces.Repositories;

/// <summary>
/// Append-only persistence boundary for the <see cref="ModerationAction"/> audit
/// trail. There is deliberately no update or delete member — audit records are
/// immutable (product-workflow-context.md section 4).
/// </summary>
public interface IModerationActionRepository
{
    Task<IReadOnlyList<ModerationAction>> GetByReportIdAsync(long reportId, CancellationToken cancellationToken = default);

    Task AddAsync(ModerationAction action, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
