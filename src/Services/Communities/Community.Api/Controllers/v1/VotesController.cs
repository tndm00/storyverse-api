namespace Community.Api.Controllers.v1;

/// <summary>
/// Weekly ranking votes. Casting a vote is idempotent within the current ISO
/// week; the tally is public.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.VotesBase)]
public sealed class VotesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VotesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Cast the caller's vote for a story this week. Returns 200 even if already voted.</summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<CastVoteResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CastVote(
        [FromBody] CastVoteRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CastVoteCommand { StoryId = request.StoryId }, cancellationToken);

        return Ok(ResponseDto<CastVoteResultDto>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.VotesCountSegment)]
    [ProducesResponseType(typeof(ResponseDto<VoteCountResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeekVoteCount(
        [FromQuery(Name = "story-id")] Guid storyId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetWeekVoteCountQuery { StoryId = storyId }, cancellationToken);

        return Ok(ResponseDto<VoteCountResponseDto>.Ok(result));
    }

    /// <summary>Current weekly voting period boundaries (Monday 00:00 UTC .. next Monday 00:00 UTC).</summary>
    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.VotesCurrentPeriodSegment)]
    [ProducesResponseType(typeof(ResponseDto<VotePeriodResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentVotePeriod(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentVotePeriodQuery(), cancellationToken);

        return Ok(ResponseDto<VotePeriodResponseDto>.Ok(result));
    }
}
