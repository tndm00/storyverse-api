namespace Moderation.Infrastructure.Repositories;

/// <summary>EF Core-backed implementation of <see cref="IModerationActionRepository"/>.</summary>
public sealed class ModerationActionRepository : IModerationActionRepository
{
    private readonly ModerationDbContext _dbContext;

    public ModerationActionRepository(ModerationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Reads the full action history for a report, oldest first, without change tracking.</summary>
    public async Task<IReadOnlyList<ModerationAction>> GetByReportIdAsync(
        long reportId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ModerationActions
            .AsNoTracking()
            .Where(x => x.ReportId == reportId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Stages a new moderation action for insert; not persisted until <see cref="SaveChangesAsync"/> runs.</summary>
    public async Task AddAsync(ModerationAction action, CancellationToken cancellationToken = default)
    {
        await _dbContext.ModerationActions.AddAsync(action, cancellationToken);
    }

    /// <summary>Persists all pending changes tracked by the context.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
