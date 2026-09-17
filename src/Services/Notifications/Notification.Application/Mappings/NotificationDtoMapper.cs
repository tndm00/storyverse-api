namespace Notification.Application.Mappings;

/// <summary>
/// Explicit entity-to-response-DTO projection for the Notification service.
/// Entities are never returned directly as API contracts (code-standard.md
/// section 19); this is the single place that translates them.
/// </summary>
public static class NotificationDtoMapper
{
    /// <summary>Projects a notification entity to its API response DTO.</summary>
    public static NotificationResponseDto ToDto(NotificationEntity notification)
    {
        return new NotificationResponseDto
        {
            Id = notification.PublicId,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Body = notification.Body,
            RefType = notification.RefType,
            RefId = notification.RefId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }
}
