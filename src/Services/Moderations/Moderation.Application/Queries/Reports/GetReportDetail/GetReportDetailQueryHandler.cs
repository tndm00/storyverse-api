namespace Moderation.Application.Queries.Reports.GetReportDetail;

public sealed class GetReportDetailQueryHandler : IQueryHandler<GetReportDetailQuery, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;
    private readonly IReportEnricher _enricher;

    public GetReportDetailQueryHandler(
        IReportRepository reportRepository,
        IModerationActionRepository actionRepository,
        IReportEnricher enricher)
    {
        _reportRepository = reportRepository;
        _actionRepository = actionRepository;
        _enricher = enricher;
    }

    public async Task<ReportDetailResponseDto> Handle(GetReportDetailQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByPublicIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReportNotFound);

        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);

        var enrichment = await _enricher.EnrichAsync(new[] { report }, cancellationToken);

        return ModerationDtoMapper.ToDetail(
            report,
            actions,
            enrichment.ReporterNameFor(report.ReporterUserId),
            enrichment.TargetTitleFor(report.TargetId));
    }
}
