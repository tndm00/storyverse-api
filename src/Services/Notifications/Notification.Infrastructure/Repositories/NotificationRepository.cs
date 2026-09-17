namespace Notification.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Returns a newest-first page of one user's notifications, optionally filtered by read state, plus the total match count.</summary>
    public async Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> GetForUserAsync(
        long userId,
        bool? isRead,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Scope to this recipient only; reads never leak another user's notifications.
        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        // Optionally narrow to read/unread only.
        if (isRead.HasValue)
        {
            query = query.Where(x => x.IsRead == isRead.Value);
        }

        // Total count is computed before paging so it reflects the full filtered set.
        var totalCount = await query.CountAsync(cancellationToken);

        // Newest first, with Id as a stable tiebreaker for equal timestamps.
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Looks up a single notification by its public id, or null when none exists.</summary>
    public Task<NotificationEntity> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    /// <summary>Counts a user's unread notifications.</summary>
    public Task<int> CountUnreadAsync(long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
    }

    /// <summary>Bulk-updates every unread notification owned by the user to read, in a single statement.</summary>
    public Task<int> MarkAllReadAsync(long userId, DateTime readAt, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.IsRead, true)
                    .SetProperty(x => x.ReadAt, readAt)
                    .SetProperty(x => x.UpdatedAt, readAt),
                cancellationToken);
    }

    /// <summary>Stages a new notification for insert.</summary>
    public async Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
    }

    /// <summary>Stages an existing notification's changes for update.</summary>
    public void Update(NotificationEntity notification)
    {
        _dbContext.Notifications.Update(notification);
    }

    /// <summary>Persists all staged changes to the database.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
