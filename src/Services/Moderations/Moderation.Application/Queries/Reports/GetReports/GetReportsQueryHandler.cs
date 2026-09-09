namespace Moderation.Application.Queries.Reports.GetReports;

public sealed class GetReportsQueryHandler
    : IQueryHandler<GetReportsQuery, PagedResponseDto<ReportSummaryResponseDto>>
{
    private readonly IReportRepository _reportRepository;

    public GetReportsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
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
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var (items, totalCount) = await _reportRepository.SearchAsync(criteria, cancellationToken);

        var summaries = items.Select(ModerationDtoMapper.ToSummary).ToArray();

        return PagedResponseDto<ReportSummaryResponseDto>.Create(summaries, pageNumber, pageSize, totalCount);
    }

    private static TEnum? ParseEnum<TEnum>(string value) where TEnum : struct, Enum
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsed) ? parsed : null;
    }
}
