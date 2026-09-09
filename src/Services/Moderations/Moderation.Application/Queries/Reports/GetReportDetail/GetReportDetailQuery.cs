namespace Moderation.Application.Queries.Reports.GetReportDetail;

/// <summary>Full report detail plus its moderation-action audit trail.</summary>
public sealed class GetReportDetailQuery : IQuery<ReportDetailResponseDto>
{
    public Guid ReportId { get; init; }
}
