namespace Notification.Application.Queries.Notifications.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler
    : IQueryHandler<GetMyNotificationsQuery, PagedResponseDto<NotificationResponseDto>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserContext _currentUser;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserContext currentUser)
    {
        _notificationRepository = notificationRepository;
        _currentUser = currentUser;
    }

    public async Task<PagedResponseDto<NotificationResponseDto>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        var (items, totalCount) = await _notificationRepository.GetForUserAsync(
            userId,
            request.IsRead,
            pageNumber,
            pageSize,
            cancellationToken);

        var dtos = items.Select(NotificationDtoMapper.ToDto).ToArray();

        return PagedResponseDto<NotificationResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
