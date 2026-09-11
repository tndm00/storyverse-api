namespace Community.Api.Controllers.v1;

/// <summary>
/// Chapter comment threads. Reading is public; writing requires an authenticated
/// user and is always attributed to the JWT subject. Hide/unhide require the
/// <c>community.moderate</c> permission.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.CommentsBase)]
public sealed class CommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Paged list of visible comments for a chapter, newest first.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<CommentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChapterComments(
        [FromQuery(Name = "chapter-id")] Guid chapterId,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChapterCommentsQuery
        {
            ChapterId = chapterId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<CommentResponseDto>>.Ok(result));
    }

    /// <summary>
    /// Cross-platform feed: the most recent visible comments across every
    /// chapter, newest first, enriched with story/chapter context — powers a
    /// "recently commented stories" section. Default 15, capped at 50.
    /// </summary>
    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.CommentRecentSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<RecentCommentEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentComments(
        [FromQuery(Name = "limit")] int limit = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetRecentCommentsQuery { Limit = limit }, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<RecentCommentEntryDto>>.Ok(result));
    }

    /// <summary>
    /// Cross-chapter comment moderation listing for the admin console. Filters:
    /// <c>status</c> (Visible/Hidden/Deleted/all), <c>q</c> (ILIKE content),
    /// <c>chapter-id</c>, <c>author-user-id</c>. Requires <c>community.moderate</c>.
    /// </summary>
    [HasPermission(StoryVersePermissions.Community.Moderate)]
    [HttpGet(ControllerRouteConstants.CommentAdminSegment)]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<CommentResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAdminComments(
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "q")] string q,
        [FromQuery(Name = "chapter-id")] Guid? chapterId,
        [FromQuery(Name = "author-user-id")] long? authorUserId,
        [FromQuery(Name = "sort-by")] string sortBy,
        [FromQuery(Name = "sort-direction")] string sortDirection,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminCommentsQuery
        {
            Status = status,
            Keyword = q,
            ChapterId = chapterId,
            AuthorUserId = authorUserId,
            SortBy = sortBy,
            SortDirection = sortDirection,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<CommentResponseDto>>.Ok(result));
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<CommentResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddComment(
        [FromBody] AddCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new AddCommentCommand { ChapterId = request.ChapterId, Content = request.Content };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<CommentResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPost(ControllerRouteConstants.CommentRepliesSegment)]
    [ProducesResponseType(typeof(ResponseDto<CommentResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> ReplyToComment(
        Guid commentId,
        [FromBody] ReplyCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ReplyCommentCommand { ParentCommentId = commentId, Content = request.Content };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<CommentResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPut(ControllerRouteConstants.CommentByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<CommentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EditComment(
        Guid commentId,
        [FromBody] EditCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new EditCommentCommand { CommentId = commentId, Content = request.Content };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<CommentResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpDelete(ControllerRouteConstants.CommentByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<CommentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteCommentCommand { CommentId = commentId }, cancellationToken);

        return Ok(ResponseDto<CommentResponseDto>.Ok(result));
    }

    [HasPermission(StoryVersePermissions.Community.Moderate)]
    [HttpPost(ControllerRouteConstants.CommentHideSegment)]
    [ProducesResponseType(typeof(ResponseDto<CommentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> HideComment(Guid commentId, CancellationToken cancellationToken)
    {
        var command = new SetCommentVisibilityCommand { CommentId = commentId, Hide = true };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<CommentResponseDto>.Ok(result));
    }

    [HasPermission(StoryVersePermissions.Community.Moderate)]
    [HttpPost(ControllerRouteConstants.CommentUnhideSegment)]
    [ProducesResponseType(typeof(ResponseDto<CommentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UnhideComment(Guid commentId, CancellationToken cancellationToken)
    {
        var command = new SetCommentVisibilityCommand { CommentId = commentId, Hide = false };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<CommentResponseDto>.Ok(result));
    }

    /// <summary>
    /// Internal, service-to-service: the Moderation service applies a Hide/Remove
    /// (or restore) report decision to a comment. Authorized by a valid JWT OR the
    /// <c>X-Service-Token</c> header. Same effect as hide/unhide above.
    /// </summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpPost(ControllerRouteConstants.CommentModerationVisibilitySegment)]
    [ProducesResponseType(typeof(ResponseDto<CommentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetCommentModerationVisibility(
        Guid commentId,
        [FromBody] ModerationVisibilityRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new SetCommentVisibilityCommand { CommentId = commentId, Hide = request.Hidden };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<CommentResponseDto>.Ok(result));
    }

    /// <summary>
    /// Internal, service-to-service: batch comment public id -&gt; content excerpt
    /// (<c>?ids=guid,guid</c>), for the Moderation reports queue. Authorized by a
    /// valid JWT OR the <c>X-Service-Token</c> header. Unknown ids are omitted.
    /// </summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpGet(ControllerRouteConstants.CommentExcerptsSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<CommentExcerptEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCommentExcerpts(
        [FromQuery(Name = "ids")] string ids,
        CancellationToken cancellationToken)
    {
        var parsed = (ids ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => Guid.TryParse(part, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToArray();

        var result = await _mediator.Send(new GetCommentExcerptsQuery { Ids = parsed }, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<CommentExcerptEntryDto>>.Ok(result));
    }
}
