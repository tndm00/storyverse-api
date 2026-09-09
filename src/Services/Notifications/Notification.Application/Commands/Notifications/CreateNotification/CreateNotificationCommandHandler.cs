namespace Notification.Application.Commands.Notifications.CreateNotification;

public sealed class CreateNotificationCommandHandler : ICommandHandler<CreateNotificationCommand, NotificationResponseDto>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<CreateNotificationCommandHandler> _logger;

    public CreateNotificationCommandHandler(
        INotificationRepository notificationRepository,
        ILogger<CreateNotificationCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<NotificationResponseDto> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var notification = new NotificationEntity
        {
            UserId = request.UserId,
            Type = request.Type,
            Title = request.Title.Trim(),
            Body = request.Body.Trim(),
            RefType = string.IsNullOrWhiteSpace(request.RefType) ? null : request.RefType.Trim(),
            RefId = request.RefId,
            IsRead = false,
            CreatedAt = now
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            ApplicationLogConstants.NotificationCreated,
            notification.PublicId,
            notification.Type,
            notification.UserId);

        return NotificationDtoMapper.ToDto(notification);
    }
}
