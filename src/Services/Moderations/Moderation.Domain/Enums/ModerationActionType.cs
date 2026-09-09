namespace Moderation.Domain.Enums;

/// <summary>
/// Decision a moderator recorded on a target, per product-workflow-context.md
/// section 4 (ModerationAction entity). <see cref="Warn"/>, <see cref="Hide"/>
/// and <see cref="Remove"/> resolve a report; <see cref="Dismiss"/> closes it
/// with no action against the content.
/// </summary>
public enum ModerationActionType
{
    Warn,
    Hide,
    Remove,
    Dismiss
}
