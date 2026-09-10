namespace Notification.Api.Controllers.v1;

/// <summary>
/// The current user's notification feed and read-state management. Stays thin:
/// binds the request, sends a command/query through MediatR, wraps the result in
/// <see cref="ResponseDto{T}"/>. The recipient is always taken from the JWT
/// <c>sub</c> claim, never from request input (auth-guidelines.md section 3).
/// </summary>
[ApiController]
[Authorize]
[Route(ControllerRouteConstants.NotificationsBase)]
public sealed class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>My notifications, newest first, paged, with an optional read-state filter.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<NotificationResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery(Name = "is-read")] bool? isRead,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyNotificationsQuery
        {
            IsRead = isRead,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<NotificationResponseDto>>.Ok(result));
    }

    /// <summary>Number of unread notifications for the current user.</summary>
    [HttpGet(ControllerRouteConstants.UnreadCountSegment)]
    [ProducesResponseType(typeof(ResponseDto<UnreadCountResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUnreadCountQuery(), cancellationToken);

        return Ok(ResponseDto<UnreadCountResponseDto>.Ok(result));
    }

    /// <summary>Marks one of my notifications read.</summary>
    [HttpPost(ControllerRouteConstants.NotificationReadSegment)]
    [ProducesResponseType(typeof(ResponseDto<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new MarkNotificationReadCommand { NotificationId = notificationId },
            cancellationToken);

        return Ok(ResponseDto<NotificationResponseDto>.Ok(result));
    }

    /// <summary>Marks all of my notifications read.</summary>
    [HttpPost(ControllerRouteConstants.ReadAllSegment)]
    [ProducesResponseType(typeof(ResponseDto<UnreadCountResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkAllNotificationsReadCommand(), cancellationToken);

        return Ok(ResponseDto<UnreadCountResponseDto>.Ok(result));
    }

    /// <summary>
    /// Internal notification creation. Not for end users.
    /// </summary>
    /// <remarks>
    /// Integration point: this is where a <c>ChapterPublished</c> /
    /// <c>CommentReplied</c> / <c>ReportResolved</c> event handler would call
    /// <see cref="CreateNotificationCommand"/> once the event bus is implemented
    /// (it is interfaces-only today — domain spec section 5.3). Until then the
    /// command is reachable only through this endpoint.
    /// </remarks>
    [HttpPost]
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [ProducesResponseType(typeof(ResponseDto<NotificationResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateNotification(
        [FromBody] CreateNotificationRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<NotificationType>(request.Type, ignoreCase: true, out var type))
        {
            throw new BadRequestException(ApplicationErrorConstants.InvalidNotificationType);
        }

        var command = new CreateNotificationCommand
        {
            UserId = request.UserId,
            Type = type,
            Title = request.Title,
            Body = request.Body,
            RefType = request.RefType,
            RefId = request.RefId
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<NotificationResponseDto>.Ok(result));
    }
}
