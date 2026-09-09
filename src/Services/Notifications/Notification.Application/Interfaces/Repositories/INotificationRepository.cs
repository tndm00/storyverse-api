namespace Notification.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="NotificationEntity"/>. All reads are
/// scoped to a single recipient; this service never returns another user's
/// notifications.
/// </summary>
public interface INotificationRepository
{
    /// <summary>Newest-first page of one user's notifications, optionally filtered by read state.</summary>
    Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> GetForUserAsync(
        long userId,
        bool? isRead,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<NotificationEntity> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);

    Task<int> CountUnreadAsync(long userId, CancellationToken cancellationToken = default);

    /// <summary>Marks every unread notification for the user read; returns the number affected.</summary>
    Task<int> MarkAllReadAsync(long userId, DateTime readAt, CancellationToken cancellationToken = default);

    Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default);

    void Update(NotificationEntity notification);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
