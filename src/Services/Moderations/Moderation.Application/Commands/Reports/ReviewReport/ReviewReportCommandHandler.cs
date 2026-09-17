namespace Moderation.Application.Commands.Reports.ReviewReport;

/// <summary>Handles <see cref="ReviewReportCommand"/>: moves a report from <c>Pending</c> to <c>Reviewing</c>.</summary>
public sealed class ReviewReportCommandHandler : ICommandHandler<ReviewReportCommand, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<ReviewReportCommandHandler> _logger;

    /// <summary>Creates the handler with the repositories, current-user context, and logger it needs.</summary>
    public ReviewReportCommandHandler(
        IReportRepository reportRepository,
        IModerationActionRepository actionRepository,
        ICurrentUserContext currentUser,
        ILogger<ReviewReportCommandHandler> logger)
    {
        _reportRepository = reportRepository;
        _actionRepository = actionRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Picks a pending report up for review, moving its status to <c>Reviewing</c>.</summary>
    public async Task<ReportDetailResponseDto> Handle(ReviewReportCommand request, CancellationToken cancellationToken)
    {
        var moderatorUserId = _currentUser.GetUserId();

        // Load the report; fail fast if it doesn't exist.
        var report = await _reportRepository.GetByPublicIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReportNotFound);

        // Only a pending report can be picked up for review.
        if (!ReportStatusPolicy.CanReview(report.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.ReportNotPending);
        }

        report.Status = ReportStatus.Reviewing;
        report.UpdatedAt = DateTime.UtcNow;

        _reportRepository.Update(report);
        await _reportRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ReportPickedUp, report.Id, moderatorUserId);

        // Re-fetch the action history to build the detail response.
        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);
        return ModerationDtoMapper.ToDetail(report, actions);
    }
}
