namespace Moderation.Application.Interfaces.Repositories;

/// <summary>Persistence boundary for the user-submitted <see cref="Report"/> queue.</summary>
public interface IReportRepository
{
    /// <summary>Fetches a report by its public (external-facing) id, or null when not found.</summary>
    Task<Report> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    /// <summary>Paged moderation queue, newest first, optionally filtered by status and reason.</summary>
    Task<(IReadOnlyList<Report> Items, int TotalCount)> SearchAsync(
        ReportSearchCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>Queues a new report for insertion.</summary>
    Task AddAsync(Report report, CancellationToken cancellationToken = default);

    /// <summary>Marks a tracked report as modified.</summary>
    void Update(Report report);

    /// <summary>Persists queued changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
