namespace Moderation.Domain.Entities;

/// <summary>
/// Immutable audit record of a moderator decision. Written once and NEVER
/// updated or deleted (product-workflow-context.md section 4 — ModerationAction
/// is the moderation audit trail). A null <see cref="ReportId"/> denotes
/// proactive moderation not tied to any user report.
/// </summary>
public sealed class ModerationAction : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Owning report row id, or null for proactive moderation.</summary>
    public long? ReportId { get; set; }

    /// <summary>User id of the acting moderator, taken from the JWT <c>sub</c> claim.</summary>
    public long ModeratorUserId { get; set; }

    public ModerationTargetType TargetType { get; set; }

    public Guid TargetId { get; set; }

    public ModerationActionType Action { get; set; }

    public string Note { get; set; }
}
