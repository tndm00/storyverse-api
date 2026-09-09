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

    [Authorize]
    [HttpPost(ControllerRouteConstants.ChapterPublishSegment)]
    [ProducesResponseType(typeof(ResponseDto<ChapterDetailResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Publish(Guid chapterId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PublishChapterCommand { ChapterId = chapterId }, cancellationToken);

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
