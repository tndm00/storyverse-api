namespace Moderation.Application.Commands.Reports.ReviewReport;

/// <summary>Picks a report up off the queue: <c>Pending → Reviewing</c>.</summary>
public sealed class ReviewReportCommand : ICommand<ReportDetailResponseDto>
{
    public Guid ReportId { get; init; }
}
