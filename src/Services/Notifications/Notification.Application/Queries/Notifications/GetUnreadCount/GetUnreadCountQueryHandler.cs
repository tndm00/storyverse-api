namespace Notification.Application.Queries.Notifications.GetUnreadCount;

public sealed class GetUnreadCountQueryHandler : IQueryHandler<GetUnreadCountQuery, UnreadCountResponseDto>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserContext _currentUser;

    public GetUnreadCountQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserContext currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
    }

    public async Task<UnreadCountResponseDto> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var count = await _notificationRepository.CountUnreadAsync(userId, cancellationToken);

        return new UnreadCountResponseDto { Count = count };
    }
}
