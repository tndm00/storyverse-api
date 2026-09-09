namespace Moderation.Application.Dtos;

/// <summary>
/// Normalized inputs for the moderation queue listing. Built by
/// <c>GetReportsQueryHandler</c> after validation and clamping.
/// </summary>
public sealed record ReportSearchCriteria
{
    public ReportStatus? Status { get; init; }

    public ReportReason? Reason { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
