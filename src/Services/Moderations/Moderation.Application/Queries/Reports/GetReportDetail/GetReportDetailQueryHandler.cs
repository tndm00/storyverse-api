namespace Moderation.Application.Queries.Reports.GetReportDetail;

/// <summary>Handles <see cref="GetReportDetailQuery"/>: fetches a single report's full detail and audit trail.</summary>
public sealed class GetReportDetailQueryHandler : IQueryHandler<GetReportDetailQuery, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;
    private readonly IReportEnricher _enricher;

    /// <summary>Creates the handler with the repositories and the report enricher it needs.</summary>
    public GetReportDetailQueryHandler(
        IReportRepository reportRepository,
        IModerationActionRepository actionRepository,
        IReportEnricher enricher)
    {
        _reportRepository = reportRepository;
        _actionRepository = actionRepository;
        _enricher = enricher;
    }

    /// <summary>Fetches a report by id, its action history, and enriches it with reporter/target display data.</summary>
    public async Task<ReportDetailResponseDto> Handle(GetReportDetailQuery request, CancellationToken cancellationToken)
    {
        // Load the report; fail fast if it doesn't exist.
        var report = await _reportRepository.GetByPublicIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReportNotFound);

        // Load its full moderation-action audit trail.
        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);

        // Best-effort enrichment: reporter display name and target title from other services.
        var enrichment = await _enricher.EnrichAsync(new[] { report }, cancellationToken);

        return ModerationDtoMapper.ToDetail(
            report,
            actions,
            enrichment.ReporterNameFor(report.ReporterUserId),
            enrichment.TargetTitleFor(report.TargetId));
    }
}
