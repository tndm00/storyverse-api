namespace Moderation.Application.Queries.Reports.GetReports;

public sealed class GetReportsQueryHandler
    : IQueryHandler<GetReportsQuery, PagedResponseDto<ReportSummaryResponseDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportEnricher _enricher;

    public GetReportsQueryHandler(IReportRepository reportRepository, IReportEnricher enricher)
    {
        _reportRepository = reportRepository;
        _enricher = enricher;
    }

    public async Task<PagedResponseDto<ReportSummaryResponseDto>> Handle(
        GetReportsQuery request,
        CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        var criteria = new ReportSearchCriteria
        {
            Status = ParseEnum<ReportStatus>(request.Status),
            Reason = ParseEnum<ReportReason>(request.Reason),
            SearchTerm = string.IsNullOrWhiteSpace(request.Query) ? null : request.Query.Trim(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _reportRepository.SearchAsync(criteria, cancellationToken);

        var enrichment = await _enricher.EnrichAsync(items, cancellationToken);

        var summaries = items
            .Select(report => ModerationDtoMapper.ToSummary(
                report,
                enrichment.ReporterNameFor(report.ReporterUserId),
                enrichment.TargetTitleFor(report.TargetId)))
            .ToArray();

        return PagedResponseDto<ReportSummaryResponseDto>.Create(summaries, pageNumber, pageSize, totalCount);
    }

    private static TEnum? ParseEnum<TEnum>(string value) where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : null;
    }
}
