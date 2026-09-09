namespace Notification.Application.Queries.Notifications.GetMyNotifications;

/// <summary>
/// The current user's notification feed, newest first, paged, with an optional
/// read-state filter. Identity comes from the JWT <c>sub</c> claim, never from
/// the request.
/// </summary>
public sealed class GetMyNotificationsQuery : IQuery<PagedResponseDto<NotificationResponseDto>>
{
    public bool? IsRead { get; init; }

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = ApplicationConstants.DefaultPageSize;
}
