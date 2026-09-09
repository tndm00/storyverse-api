namespace Library.Api.Controllers.v1;

/// <summary>
/// Automatic reading history powering "đọc tiếp" (continue reading). Written
/// whenever a chapter is opened; scoped to the authenticated caller.
/// </summary>
[ApiController]
[Authorize]
[Route(ControllerRouteConstants.ReadingProgressBase)]
public sealed class ReadingProgressController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReadingProgressController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>The caller's most recently read stories, newest first.</summary>
    [HttpGet(ControllerRouteConstants.ReadingProgressContinueSegment)]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<ReadingProgressResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContinueReading(
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetContinueReadingQuery { PageNumber = pageNumber, PageSize = pageSize };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<ReadingProgressResponseDto>>.Ok(result));
    }

    /// <summary>The caller's last reading position in one story.</summary>
    [HttpGet(ControllerRouteConstants.ReadingProgressByStorySegment)]
    [ProducesResponseType(typeof(ResponseDto<ReadingProgressResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStoryProgress(Guid storyId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStoryProgressQuery { StoryId = storyId }, cancellationToken);

        return Ok(ResponseDto<ReadingProgressResponseDto>.Ok(result));
    }

    /// <summary>Record or advance the caller's position in a story (called when a chapter is opened).</summary>
    [HttpPut(ControllerRouteConstants.ReadingProgressByStorySegment)]
    [ProducesResponseType(typeof(ResponseDto<ReadingProgressResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertProgress(
        Guid storyId,
        [FromBody] UpsertReadingProgressRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpsertReadingProgressCommand
        {
            StoryId = storyId,
            LastChapterId = request.LastChapterId,
            ScrollPercent = request.ScrollPercent
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<ReadingProgressResponseDto>.Ok(result));
    }
}
