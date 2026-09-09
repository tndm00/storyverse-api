namespace Moderation.Domain.Enums;

/// <summary>
/// Lifecycle of a <see cref="Entities.Report"/> in the moderation queue, per
/// product-workflow-context.md section 7 (state machine
/// <c>Pending → Reviewing → Resolved | Dismissed</c>). Phase 1 only.
/// </summary>
public enum ReportStatus
{
    Pending,
    Reviewing,
    Resolved,
    Dismissed
}
