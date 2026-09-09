namespace Moderation.Application.Commands.Reports.ReviewReport;

public sealed class ReviewReportCommandHandler : ICommandHandler<ReviewReportCommand, ReportDetailResponseDto>
{
    private readonly IReportRepository _reportRepository;
    private readonly IModerationActionRepository _actionRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<ReviewReportCommandHandler> _logger;

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

    public async Task<ReportDetailResponseDto> Handle(ReviewReportCommand request, CancellationToken cancellationToken)
    {
        var moderatorUserId = _currentUser.GetUserId();

        var report = await _reportRepository.GetByPublicIdAsync(request.ReportId, cancellationToken)
            ?? throw new NotFoundException(ApplicationErrorConstants.ReportNotFound);

        if (!ReportStatusPolicy.CanReview(report.Status))
        {
            throw new BusinessRuleException(ApplicationErrorConstants.ReportNotPending);
        }

        report.Status = ReportStatus.Reviewing;
        report.UpdatedAt = DateTime.UtcNow;

        _reportRepository.Update(report);
        await _reportRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.ReportPickedUp, report.Id, moderatorUserId);

        var actions = await _actionRepository.GetByReportIdAsync(report.Id, cancellationToken);
        return ModerationDtoMapper.ToDetail(report, actions);
    }
}
