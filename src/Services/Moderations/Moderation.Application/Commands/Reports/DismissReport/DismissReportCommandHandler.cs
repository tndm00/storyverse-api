namespace Moderation.Application.Commands.Reports.DismissReport;

/// <summary>Handles <see cref="DismissReportCommand"/>: closes a report with no action taken against the content.</summary>
public sealed class DismissReportCommandHandler : ICommandHandler<DismissReportCommand, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;
    private readonly IModerationUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<DismissReportCommandHandler> _logger;

    /// <summary>Creates the handler with the repositories, unit of work, current-user context, and logger it needs.</summary>
    public DismissReportCommandHandler(
        IReportRepository reportRepository,
        IModerationActionRepository actionRepository,
        IModerationUnitOfWork unitOfWork,
        ICurrentUserContext currentUser,
        ILogger<DismissReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _actionRepository = actionRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Dismisses a report: records a <c>Dismiss</c> action and marks the report closed.</summary>
    public async Task<ReportDetailResponseDto> Handle(DismissReportCommand request, CancellationToken cancellationToken)
    {
        var moderatorUserId = _currentUser.GetUserId();

        // Load the report; fail fast if it doesn't exist.
        var report = await _reportRepository.GetByPublicIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReportNotFound);

        // A report already closed (resolved/dismissed) cannot be dismissed again.
        if (!ReportStatusPolicy.CanClose(report.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.ReportAlreadyClosed);
        }

        var now = DateTime.UtcNow;

        // Build the moderation action record documenting the dismissal.
        var action = new ModerationAction
        {
            ReportId = report.Id,
            ModeratorUserId = moderatorUserId,
            TargetType = report.TargetType,
            TargetId = report.TargetId,
            Action = ModerationActionType.Dismiss,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = now
        };

        // Close the report.
        report.Status = ReportStatus.Dismissed;
        report.ResolvedAt = now;
        report.UpdatedAt = now;

        // Persist the new action and the updated report status atomically.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _actionRepository.AddAsync(action, ct);
            _reportRepository.Update(report);
            await _actionRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.ReportDismissed, report.Id, moderatorUserId, action.Id);

        // Re-fetch the full action history to build the detail response.
        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);
        return ModerationDtoMapper.ToDetail(report, actions);
    }
}
