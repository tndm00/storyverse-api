namespace Moderation.Application.Policies;

/// <summary>
/// The report state machine from product-workflow-context.md section 7:
/// <c>Pending → Reviewing → Resolved | Dismissed</c>. A moderator may also
/// resolve or dismiss straight from <c>Pending</c> without an explicit pick-up.
/// </summary>
public static class ReportStatusPolicy
{
    /// <summary>Only a pending report can be picked up into review.</summary>
    public static bool CanReview(ReportStatus status) => status == ReportStatus.Pending;

    /// <summary>A report can be closed (resolved or dismissed) while still open.</summary>
    public static bool CanClose(ReportStatus status) =>
        status is ReportStatus.Pending or ReportStatus.Reviewing;
}
