namespace Moderation.Domain.Entities;

/// <summary>
/// A user-submitted content-violation report. Enters the moderation queue as
/// <see cref="ReportStatus.Pending"/> and is never auto-actioned in Phase 1
/// (product-workflow-context.md section 7.5 — reports do not auto-hide content;
/// no severe-violation auto-rules yet). A moderator drives it through
/// <see cref="ReportStatus.Reviewing"/> to <see cref="ReportStatus.Resolved"/>
/// or <see cref="ReportStatus.Dismissed"/>.
/// </summary>
public sealed class Report : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>User id of the reporter, taken from the JWT <c>sub</c> claim — never from request input.</summary>
    public long ReporterUserId { get; set; }

    public ModerationTargetType TargetType { get; set; }

    /// <summary>Public id of the reported object, owned by another service.</summary>
    public Guid TargetId { get; set; }

    public ReportReason Reason { get; set; }

    public string Description { get; set; }

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    /// <summary>Set when the report reaches <see cref="ReportStatus.Resolved"/> or <see cref="ReportStatus.Dismissed"/>.</summary>
    public DateTime? ResolvedAt { get; set; }
}
