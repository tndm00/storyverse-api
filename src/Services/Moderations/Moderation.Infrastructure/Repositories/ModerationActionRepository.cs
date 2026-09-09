namespace Moderation.Infrastructure.Repositories;

public sealed class ModerationActionRepository : IModerationActionRepository
{
    private readonly ModerationDbContext _dbContext;

    public ModerationActionRepository(ModerationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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

    public async Task AddAsync(ModerationAction action, CancellationToken cancellationToken = default)
    {
        await _dbContext.ModerationActions.AddAsync(action, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
