namespace Moderation.Infrastructure.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly ModerationDbContext _dbContext;

    public ReportRepository(ModerationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Report> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Reports.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Report> Items, int TotalCount)> SearchAsync(
        ReportSearchCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Reports.AsNoTracking().AsQueryable();

        if (criteria.Status is { } status)
        {
            query = query.Where(x => x.Status == status);
        }

        if (criteria.Reason is { } reason)
        {
            query = query.Where(x => x.Reason == reason);
        }

        if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
        {
            var term = criteria.SearchTerm.Trim();
            var like = $"%{term}%";

            // A term that parses as a GUID targets the report or its content by id;
            // an enum-name term narrows by reason/status; anything else is a
            // substring match on the reporter's free-text description.
            Guid.TryParse(term, out var termGuid);
            var reasonMatch = Enum.TryParse<ReportReason>(term, ignoreCase: true, out var r) ? r : (ReportReason?)null;
            var statusMatch = Enum.TryParse<ReportStatus>(term, ignoreCase: true, out var s) ? s : (ReportStatus?)null;

            query = query.Where(x =>
                (termGuid != Guid.Empty && (x.PublicId == termGuid || x.TargetId == termGuid))
                || (x.Description != null && EF.Functions.ILike(x.Description, like))
                || (reasonMatch != null && x.Reason == reasonMatch)
                || (statusMatch != null && x.Status == statusMatch));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((criteria.PageNumber - 1) * criteria.PageSize)
            .Take(criteria.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _dbContext.Reports.AddAsync(report, cancellationToken);
    }

    public void Update(Report report)
    {
        _dbContext.Reports.Update(report);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
