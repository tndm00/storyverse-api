namespace Community.Api.Controllers.v1;

/// <summary>
/// Per-story star ratings. Listing a story's ratings is public; reading or
/// writing "my" rating requires an authenticated user. Identity comes from the
/// JWT, never the request body.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.RatingsBase)]
public sealed class RatingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RatingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Create or replace the caller's rating for a story.</summary>
    [Authorize]
    [HttpPut]
    [ProducesResponseType(typeof(ResponseDto<RatingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertMyRating(
        [FromBody] UpsertRatingRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpsertRatingCommand
        {
            StoryId = request.StoryId,
            Score = request.Score,
            ReviewText = request.ReviewText
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<RatingResponseDto>.Ok(result));
    }

    [Authorize]
    [HttpGet(ControllerRouteConstants.RatingsMineSegment)]
    [ProducesResponseType(typeof(ResponseDto<RatingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyRating(
        [FromQuery(Name = "story-id")] Guid storyId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyRatingQuery { StoryId = storyId }, cancellationToken);

        return Ok(ResponseDto<RatingResponseDto>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<RatingResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStoryRatings(
        [FromQuery(Name = "story-id")] Guid storyId,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoryRatingsQuery
        {
            StoryId = storyId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<RatingResponseDto>>.Ok(result));
    }
}
