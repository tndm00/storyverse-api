namespace Moderation.Application.Commands.Reports.SubmitReport;

/// <summary>
/// Files a content-violation report. Any authenticated user may submit one; it
/// enters the queue as <see cref="ReportStatus.Pending"/> and does not
/// auto-hide the target (product-workflow-context.md section 7.5).
/// </summary>
public sealed class SubmitReportCommand : ICommand<ReportDetailResponseDto>
{
    public ModerationTargetType TargetType { get; init; }

    public Guid TargetId { get; init; }

    public ReportReason Reason { get; init; }

    public string Description { get; init; }
}
