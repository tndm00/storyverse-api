namespace Notification.Application.Commands.Notifications.MarkAllNotificationsRead;

public sealed class MarkAllNotificationsReadCommandHandler
    : ICommandHandler<MarkAllNotificationsReadCommand, UnreadCountResponseDto>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<MarkAllNotificationsReadCommandHandler> _logger;

    public MarkAllNotificationsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserContext currentUser,
        ILogger<MarkAllNotificationsReadCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<UnreadCountResponseDto> Handle(
        MarkAllNotificationsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var affected = await _notificationRepository.MarkAllReadAsync(userId, DateTime.UtcNow, cancellationToken);

        _logger.LogInformation(ApplicationLogConstants.NotificationsAllRead, userId, affected);

        return new UnreadCountResponseDto { Count = 0 };
    }
}
