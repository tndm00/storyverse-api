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

    /// <summary>Fetches a page of the current user's notifications, normalizing the paging parameters first.</summary>
    public async Task<PagedResponseDto<NotificationResponseDto>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.GetUserId();

        // Clamp paging inputs so an out-of-range or missing page number/size never reaches the repository.
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(
            request.PageSize <= 0 ? ApplicationConstants.DefaultPageSize : request.PageSize,
            ApplicationConstants.MinPageSize,
            ApplicationConstants.MaxPageSize);

        // Fetch the page scoped to this user, optionally filtered by read state.
        var (items, totalCount) = await _notificationRepository.GetForUserAsync(
            userId,
            request.IsRead,
            pageNumber,
            pageSize,
            cancellationToken);

        // Project entities to response DTOs and wrap them in the paging envelope.
        var dtos = items.Select(NotificationDtoMapper.ToDto).ToArray();

        return PagedResponseDto<NotificationResponseDto>.Create(dtos, pageNumber, pageSize, totalCount);
    }
}
