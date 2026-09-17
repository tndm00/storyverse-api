namespace Notification.Application.Commands.Notifications.MarkNotificationRead;

public sealed class MarkNotificationReadCommandHandler : ICommandHandler<MarkNotificationReadCommand, NotificationResponseDto>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserContext _currentUser;
    private readonly ILogger<MarkNotificationReadCommandHandler> _logger;

    public MarkNotificationReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserContext currentUser,
        ILogger<MarkNotificationReadCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>Marks a single notification owned by the current user as read, if it isn't already.</summary>
    public async Task<NotificationResponseDto> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var notification = await _notificationRepository.GetByPublicIdAsync(request.NotificationId, cancellationToken);

        // A notification that belongs to another user is indistinguishable from a
        // missing one, so the caller cannot probe another user's feed.
        if (notification is null || notification.UserId != userId)
        {
            throw new NotFoundException(ApplicationErrorConstants.NotificationNotFound);
        }

        // Only touch storage when there is an actual state change to persist.
        if (!notification.IsRead)
        {
            notification.MarkRead(DateTime.UtcNow);
            _notificationRepository.Update(notification);
            await _notificationRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(ApplicationLogConstants.NotificationRead, notification.PublicId, userId);
        }

        return NotificationDtoMapper.ToDto(notification);
    }
}
