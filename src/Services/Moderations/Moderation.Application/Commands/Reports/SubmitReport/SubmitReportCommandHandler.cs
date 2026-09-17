namespace Moderation.Application.Commands.Reports.SubmitReport;

/// <summary>Handles <see cref="SubmitReportCommand"/>: files a new content-violation report onto the queue.</summary>
public sealed class SubmitReportCommandHandler : ICommandHandler<SubmitReportCommand, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<SubmitReportCommandHandler> _logger;

    /// <summary>Creates the handler with the report repository, current-user context, and logger it needs.</summary>
    public SubmitReportCommandHandler(
        IReportRepository reportRepository,
        ICurrentUserContext currentUser,
        ILogger<SubmitReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Creates a new report in <c>Pending</c> status for the given target.</summary>
    public async Task<ReportDetailResponseDto> Handle(SubmitReportCommand request, CancellationToken cancellationToken)
    {
        var reporterUserId = _currentUser.GetUserId();

        var description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        // Build the new report record, starting in the Pending queue.
        var report = new Report
        {
            ReporterUserId = reporterUserId,
            TargetType = request.TargetType,
            TargetId = request.TargetId,
            Reason = request.Reason,
            Description = description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _reportRepository.AddAsync(report, cancellationToken);
        await _reportRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.ReportSubmitted, report.Id, report.TargetType, report.TargetId);

        // Integration point: no severe-violation auto-moderation in Phase 1
        // (product-workflow-context.md section 7.5). A future rule engine could
        // publish a "ContentFlagged" event here to auto-hide egregious targets.

        return ModerationDtoMapper.ToDetail(report, Array.Empty<ModerationAction>());
    }
}
