namespace Moderation.Application.Commands.Reports.ResolveReport;

public sealed class ResolveReportCommandHandler : ICommandHandler<ResolveReportCommand, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;
    private readonly IModerationUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;
    private readonly IContentModerationClient _contentClient;
    private readonly ICommunityModerationClient _communityClient;
    private readonly ILogger<ResolveReportCommandHandler> _logger;

    public ResolveReportCommandHandler(
        IReportRepository reportRepository,
        IModerationActionRepository actionRepository,
        IModerationUnitOfWork unitOfWork,
        ICurrentUserContext currentUser,
        IContentModerationClient contentClient,
        ICommunityModerationClient communityClient,
        ILogger<ResolveReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _actionRepository = actionRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _contentClient = contentClient;
        _communityClient = communityClient;
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

        var note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();

        // Atomicity choice (see rules): a Hide/Remove decision must actually take
        // effect on the real content before the report is marked Resolved. We call
        // the owning service FIRST; if it fails we throw and never touch the
        // report status, so the moderator can retry. The reverse ordering (commit
        // then call) could leave a "Resolved" report whose content is still live —
        // the unacceptable direction for moderation. The tolerated residual risk
        // (content hidden, then the local commit fails) leaves the effect in place
        // and the report re-resolvable, which is safe.
        if (request.Action is ModerationActionType.Hide or ModerationActionType.Remove)
        {
            await ApplyHideDecisionAsync(report, note, cancellationToken);
        }

        var action = new ModerationAction
        {
            ReportId = report.Id,
            ModeratorUserId = moderatorUserId,
            TargetType = report.TargetType,
            TargetId = report.TargetId,
            Action = request.Action,
            Note = note,
            CreatedAt = now
        };

        report.Status = ReportStatus.Resolved;
        report.ResolvedAt = now;
        report.UpdatedAt = now;

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            await _actionRepository.AddAsync(action, ct);
            _reportRepository.Update(report);
            await _actionRepository.SaveChangesAsync(ct);
        }, cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.ReportResolved, report.Id, action.Action, moderatorUserId, action.Id);

        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);
        return ModerationDtoMapper.ToDetail(report, actions);
    }

    private async Task ApplyHideDecisionAsync(Report report, string reason, CancellationToken cancellationToken)
    {
        try
        {
            switch (report.TargetType)
            {
                case ModerationTargetType.Story:
                    await _contentClient.SetStoryVisibilityAsync(report.TargetId, true, reason, cancellationToken);
                    break;
                case ModerationTargetType.Chapter:
                    await _contentClient.SetChapterVisibilityAsync(report.TargetId, true, reason, cancellationToken);
                    break;
                case ModerationTargetType.Comment:
                    await _communityClient.SetCommentVisibilityAsync(report.TargetId, true, reason, cancellationToken);
                    break;
            }

            _logger.LogInformation(
                ApplicationLogConstants.ReportModerationApplied,
                report.Id, "Hide", report.TargetType, report.TargetId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                ApplicationLogConstants.ReportModerationApplyFailed,
                report.Id, "Hide", report.TargetType, report.TargetId);

            throw new BusinessRuleException(ApplicationErrorConstants.ModerationApplyFailed);
        }
    }
}
