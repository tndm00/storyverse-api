namespace Moderation.Application.Commands.Reports.ResolveReport;

/// <summary>
/// Records a moderation action against the reported target and closes the
/// report: <c>Status → Resolved</c>, <c>ResolvedAt</c> stamped. Accepts
/// <c>Warn</c>, <c>Hide</c> or <c>Remove</c>; <c>Dismiss</c> has its own command.
/// </summary>
public sealed class ResolveReportCommand : ICommand<ReportDetailResponseDto>
{
    public Guid ReportId { get; init; }

    public ModerationActionType Action { get; init; }

    public string Note { get; init; }
}
