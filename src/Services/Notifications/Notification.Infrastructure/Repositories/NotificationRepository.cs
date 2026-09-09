namespace Notification.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<NotificationEntity> Items, int TotalCount)> GetForUserAsync(
        long userId,
        bool? isRead,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (isRead.HasValue)
        {
            query = query.Where(x => x.IsRead == isRead.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<NotificationEntity> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public Task<int> CountUnreadAsync(long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Notifications.CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
    }

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

    public async Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
    }

    public void Update(NotificationEntity notification)
    {
        _dbContext.Notifications.Update(notification);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
