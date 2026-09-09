namespace Moderation.Application.Commands.Reports.DismissReport;

public sealed class DismissReportCommandHandler : ICommandHandler<DismissReportCommand, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;
    private readonly IModerationUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<DismissReportCommandHandler> _logger;

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

    public async Task<ReportDetailResponseDto> Handle(DismissReportCommand request, CancellationToken cancellationToken)
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
            Action = ModerationActionType.Dismiss,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreatedAt = now
        };

        report.Status = ReportStatus.Dismissed;
        report.ResolvedAt = now;
        report.UpdatedAt = now;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _actionRepository.AddAsync(action, ct);
            _reportRepository.Update(report);
            await _actionRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.ReportDismissed, report.Id, moderatorUserId, action.Id);

        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);
        return ModerationDtoMapper.ToDetail(report, actions);
    }
}
