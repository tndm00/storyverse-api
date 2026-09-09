namespace Notification.Application.Dtos;

/// <summary>A notification in the recipient's feed.</summary>
public sealed class NotificationResponseDto
{
    public Guid Id { get; init; }

    public string Type { get; init; }

    public string Title { get; init; }

    public string Body { get; init; }

    public string RefType { get; init; }

    public Guid? RefId { get; init; }

    public bool IsRead { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? ReadAt { get; init; }
}

/// <summary>Unread-notification count for the current user.</summary>
public sealed class UnreadCountResponseDto
{
    public int Count { get; init; }
}

/// <summary>
/// Payload for the internal notification-creation endpoint. Normally populated
/// by an event handler in another service, not by an end user.
/// </summary>
public sealed class CreateNotificationRequestDto
{
    public long UserId { get; init; }

    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public string RefType { get; init; }

    public Guid? RefId { get; init; }
}
