namespace Content.Api.Controllers.v1;

/// <summary>Chapter reading (public) and authoring lifecycle actions (author only).</summary>
[ApiController]
[Route(ControllerRouteConstants.ChaptersBase)]
public sealed class ChaptersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChaptersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.ChapterByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChapter(Guid chapterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChapterContentQuery { ChapterId = chapterId }, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPut(ControllerRouteConstants.ChapterByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateChapter(
        Guid chapterId,
        [FromBody] UpdateChapterRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateChapterCommand
        {
            ChapterId = chapterId,
            Title = request.Title,
            OrderIndex = request.OrderIndex,
            Content = request.Content,
            VolumeId = request.VolumeId
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    /// <summary>Author submits (or resubmits after rejection) a chapter for moderation.</summary>
    [Authorize]
    [HttpPost(ControllerRouteConstants.ChapterSubmitForReviewSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitForReview(Guid chapterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SubmitChapterForReviewCommand { ChapterId = chapterId },
            cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    /// <summary>Moderation queue: chapters awaiting a decision, across every story.</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpGet(ControllerRouteConstants.ChapterPendingReviewSegment)]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<PendingReviewChapterResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingReview(
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "q")] string q,
        [FromQuery(Name = "type")] string type,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetPendingReviewChaptersQuery
            {
                Status = status,
                Keyword = q,
                Type = type,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<PendingReviewChapterResponseDto>>.Ok(result));
    }

    /// <summary>Chapters already decided on: <c>status=Approved</c> or <c>status=Rejected</c>.</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpGet(ControllerRouteConstants.ChapterReviewedSegment)]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<PendingReviewChapterResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewed(
        [FromQuery(Name = "status")] string status,
        [FromQuery(Name = "q")] string q,
        [FromQuery(Name = "type")] string type,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetReviewedChaptersQuery
            {
                Status = status,
                Keyword = q,
                Type = type,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<PendingReviewChapterResponseDto>>.Ok(result));
    }

    /// <summary>Review dashboard counters: Pending, InReview, Approved, Rejected.</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpGet(ControllerRouteConstants.ChapterReviewCountsSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterReviewCountsResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReviewCounts(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChapterReviewCountsQuery(), cancellationToken);

        return Ok(ResponseDto<ChapterReviewCountsResponseDto>.Ok(result));
    }

    /// <summary>Full chapter detail for a moderator, regardless of status or ownership.</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpGet(ControllerRouteConstants.ChapterForReviewSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForReview(Guid chapterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChapterForReviewQuery { ChapterId = chapterId }, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    /// <summary>Moderator picks a pending chapter off the queue: PendingReview -&gt; InReview.</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpPost(ControllerRouteConstants.ChapterReviewSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Review(Guid chapterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ReviewChapterCommand { ChapterId = chapterId }, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    /// <summary>Moderator approves: InReview -&gt; Published (first approval also flips the story Ongoing).</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpPost(ControllerRouteConstants.ChapterApproveSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid chapterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ApproveChapterCommand { ChapterId = chapterId }, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    /// <summary>Moderator rejects: InReview -&gt; Rejected. The author may edit and resubmit.</summary>
    [HasPermission(StoryVersePermissions.Content.Moderate)]
    [HttpPost(ControllerRouteConstants.ChapterRejectSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reject(
        Guid chapterId,
        [FromBody] RejectChapterRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new RejectChapterCommand { ChapterId = chapterId, Reason = request.Reason };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPost(ControllerRouteConstants.ChapterScheduleSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Schedule(
        Guid chapterId,
        [FromBody] ScheduleChapterRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ScheduleChapterCommand { ChapterId = chapterId, ScheduledAt = request.ScheduledAt };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPost(ControllerRouteConstants.ChapterCancelScheduleSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelSchedule(Guid chapterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelChapterScheduleCommand { ChapterId = chapterId }, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpPost(ControllerRouteConstants.ChapterRemoveSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Remove(Guid chapterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RemoveChapterCommand { ChapterId = chapterId }, cancellationToken);

        return Ok(ResponseDto<ChapterDetailResponseDto>.Ok(result));
    }
}
