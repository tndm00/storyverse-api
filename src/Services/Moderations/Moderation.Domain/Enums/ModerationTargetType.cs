namespace Moderation.Domain.Enums;

/// <summary>
/// Kind of platform object a report or moderation action points at, per
/// product-workflow-context.md section 4 (Report / ModerationAction entities).
/// The referenced row lives in another service; this service only stores the
/// discriminator plus its public id.
/// </summary>
public enum ModerationTargetType
{
    Story,
    Chapter,
    Comment
}
