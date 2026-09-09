namespace Moderation.Application.Queries.Reports.GetReportDetail;

public sealed class GetReportDetailQueryHandler : IQueryHandler<GetReportDetailQuery, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;

    public GetReportDetailQueryHandler(
        IReportRepository reportRepository,
        IModerationActionRepository actionRepository)
    {
        _reportRepository = reportRepository;
        _actionRepository = actionRepository;
    }

    public async Task<ReportDetailResponseDto> Handle(GetReportDetailQuery request, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByPublicIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReportNotFound);

        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);

        return ModerationDtoMapper.ToDetail(report, actions);
    }
}
