namespace Moderation.Application.Queries.Reports.GetReports;

/// <summary>Handles <see cref="GetReportsQuery"/>: builds the paged, filtered moderation queue listing.</summary>
public sealed class GetReportsQueryHandler
    : IQueryHandler<GetReportsQuery, PagedResponseDto<ReportSummaryResponseDto>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportEnricher _enricher;

    /// <summary>Creates the handler with the report repository and the report enricher it needs.</summary>
    public GetReportsQueryHandler(IReportRepository reportRepository, IReportEnricher enricher)
    {
        _reportRepository = reportRepository;
        _enricher = enricher;
    }

    /// <summary>Normalizes paging/filter inputs, searches reports, and enriches results with reporter/target display data.</summary>
    public async Task<PagedResponseDto<ReportSummaryResponseDto>> Handle(
        GetReportsQuery request,
        CancellationToken cancellationToken)
    {
        // Clamp paging inputs to sane bounds instead of rejecting them.
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        // Build the normalized search criteria, parsing string filters into their enum values.
        var criteria = new ReportSearchCriteria
        {
            Status = ParseEnum<ReportStatus>(request.Status),
            Reason = ParseEnum<ReportReason>(request.Reason),
            SearchTerm = string.IsNullOrWhiteSpace(request.Query) ? null : request.Query.Trim(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _reportRepository.SearchAsync(criteria, cancellationToken);

        // Best-effort enrichment: reporter display names and target titles from other services.
        var enrichment = await _enricher.EnrichAsync(items, cancellationToken);

        // Map each report entity into its summary DTO.
        var summaries = items
            .Select(report => ModerationDtoMapper.ToSummary(
                report,
                enrichment.ReporterNameFor(report.ReporterUserId),
                enrichment.TargetTitleFor(report.TargetId)))
            .ToArray();

        return PagedResponseDto<ReportSummaryResponseDto>.Create(summaries, pageNumber, pageSize, totalCount);
    }

    /// <summary>Parses a string into the given enum, case-insensitively, returning null when it doesn't match.</summary>
    private static TEnum? ParseEnum<TEnum>(string value) where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : null;
    }
}
