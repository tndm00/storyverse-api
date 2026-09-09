namespace Moderation.Application.Commands.Reports.ResolveReport;

public sealed class ResolveReportCommandHandler : ICommandHandler<ResolveReportCommand, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;
    private readonly IModerationUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<ResolveReportCommandHandler> _logger;

    public ResolveReportCommandHandler(
        IReportRepository reportRepository,
        IModerationActionRepository actionRepository,
        IModerationUnitOfWork unitOfWork,
        ICurrentUserContext currentUser,
        ILogger<ResolveReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _actionRepository = actionRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<ReportDetailResponseDto> Handle(ResolveReportCommand request, CancellationToken cancellationToken)
    {
        var moderatorUserId = _currentUser.GetUserId();

        var report = await _reportRepository.GetByPublicIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReportNotFound);

        if (!ReportStatusPolicy.CanClose(report.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.ReportAlreadyClosed);
        }

        var now = DateTime.UtcNow;

        var action = new ModerationAction
        {
            ReportId = report.Id,
            ModeratorUserId = moderatorUserId,
            TargetType = report.TargetType,
            TargetId = report.TargetId,
            Action = request.Action,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = now
        };

        report.Status = ReportStatus.Resolved;
        report.ResolvedAt = now;
        report.UpdatedAt = now;

        // Report status change and its audit row must commit together.
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _actionRepository.AddAsync(action, ct);
            _reportRepository.Update(report);
            await _actionRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.ReportResolved, report.Id, action.Action, moderatorUserId, action.Id);

        // Integration point: a Hide/Remove decision must be applied to the real
        // Chapter/Comment/Story, which lives in the Content/Community service.
        // Publish a "ContentModerated" event here once the event bus exists
        // (Phase 1A has none) so the owning service can hide or remove the target.

        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);
        return ModerationDtoMapper.ToDetail(report, actions);
    }
}
