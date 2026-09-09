namespace Notification.Domain.Entities;

/// <summary>
/// A delivered platform notification for one recipient, per the domain spec
/// sections 3-4. Creation is normally driven by events from other services
/// (e.g. a follower is notified when a chapter is published — domain spec
/// section 5.3); this service owns storage and read-state only. The recipient
/// is identified by <see cref="UserId"/>, sourced from the caller's <c>sub</c>
/// JWT claim, never from request input (auth-guidelines.md section 3).
/// </summary>
public sealed class Notification : BaseEntity
{
    /// <summary>Stable public identifier exposed by the API instead of <see cref="BaseEntity.Id"/>.</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Recipient user id. The User row lives in the Authentication service, so this
    /// is a plain value with no cross-service foreign key.
    /// </summary>
    public long UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    /// <summary>Object kind that caused the event (e.g. "chapter", "comment"); null when not applicable.</summary>
    public string RefType { get; set; }

    /// <summary>Public id of the object that caused the event; null when not applicable.</summary>
    public Guid? RefId { get; set; }

    public bool IsRead { get; set; }

    /// <summary>Set once, when the recipient first marks the notification read.</summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>Marks the notification read at <paramref name="readAt"/>; a no-op if already read.</summary>
    public void MarkRead(DateTime readAt)
    {
        if (IsRead)
        {
            return;
        }

        IsRead = true;
        ReadAt = readAt;
        UpdatedAt = readAt;
    }
}
