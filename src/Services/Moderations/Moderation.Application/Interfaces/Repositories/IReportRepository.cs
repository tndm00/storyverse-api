namespace Moderation.Application.Interfaces.Repositories;

/// <summary>Persistence boundary for the user-submitted <see cref="Report"/> queue.</summary>
public interface IReportRepository
{
    Task<Report> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    /// <summary>Paged moderation queue, newest first, optionally filtered by status and reason.</summary>
    Task<(IReadOnlyList<Report> Items, int TotalCount)> SearchAsync(
        ReportSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    Task AddAsync(Report report, CancellationToken cancellationToken = default);

    void Update(Report report);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
