namespace Notification.Domain.Enums;

/// <summary>
/// Kind of business event a <see cref="Notification.Domain.Entities.Notification"/>
/// records, per the domain spec section 3-4 (Notification entity). Stored as a
/// string column so the contract never leaks enum numbering
/// (api-guidelines.md section 10-11).
/// </summary>
public enum NotificationType
{
    /// <summary>A followed story published a new chapter (domain spec section 5.3).</summary>
    NewChapter,

    /// <summary>Someone replied to the recipient's comment.</summary>
    CommentReply,

    /// <summary>A moderation report filed by the recipient was resolved.</summary>
    ReportResult,

    /// <summary>Platform-wide announcement addressed to the recipient.</summary>
    SystemAnnouncement
}
