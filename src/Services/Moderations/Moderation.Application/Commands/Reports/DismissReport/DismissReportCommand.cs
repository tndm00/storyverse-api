namespace Moderation.Application.Commands.Reports.DismissReport;

/// <summary>
/// Closes a report with no action against the content: records a
/// <c>Dismiss</c> moderation action and sets <c>Status → Dismissed</c>.
/// </summary>
public sealed class DismissReportCommand : ICommand<ReportDetailResponseDto>
{
    public Guid ReportId { get; init; }

    public string Note { get; init; }
}
