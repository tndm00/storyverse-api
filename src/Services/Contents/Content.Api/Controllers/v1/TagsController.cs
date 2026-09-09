namespace Content.Api.Controllers.v1;

/// <summary>Free-form tag listing for discovery UIs.</summary>
[ApiController]
[Route(ControllerRouteConstants.TagsBase)]
public sealed class TagsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TagsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<TagResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPopularTags(
        [FromQuery(Name = "count")] int? count,
        CancellationToken cancellationToken)
    {
        var query = count is { } value
            ? new GetPopularTagsQuery { Count = value }
            : new GetPopularTagsQuery();

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<TagResponseDto>>.Ok(result));
    }
}
