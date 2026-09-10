namespace Moderation.Application.Dtos;

/// <summary>
/// Normalized inputs for the moderation queue listing. Built by
/// <c>GetReportsQueryHandler</c> after validation and clamping.
/// </summary>
public sealed record ReportSearchCriteria
{
    public ReportStatus? Status { get; init; }

    public ReportReason? Reason { get; init; }

    /// <summary>
    /// Free-text term from the admin queue's search box. Matched (case-insensitively)
    /// against the report/target public id, the reporter's description, and the
    /// reason/status name. Null or whitespace means "no text filter".
    /// </summary>
    public string SearchTerm { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
