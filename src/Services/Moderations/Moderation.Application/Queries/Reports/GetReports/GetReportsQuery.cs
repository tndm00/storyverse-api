namespace Moderation.Application.Queries.Reports.GetReports;

/// <summary>Moderation queue listing, newest first, paged, filterable by status and reason.</summary>
public sealed class GetReportsQuery : IQuery<PagedResponseDto<ReportSummaryResponseDto>>
{
    public string Status { get; init; }

    public string Reason { get; init; }

    /// <summary>Free-text search term (<c>q</c>): report/target id, description, reason or status.</summary>
    public string Query { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
