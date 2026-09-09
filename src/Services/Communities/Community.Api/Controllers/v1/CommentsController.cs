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
}
