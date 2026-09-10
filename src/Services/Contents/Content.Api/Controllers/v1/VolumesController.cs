namespace Content.Api.Controllers.v1;

/// <summary>Volume updates (author only). Volume creation and listing are under the story resource.</summary>
[ApiController]
[Route(ControllerRouteConstants.VolumesBase)]
public sealed class VolumesController : ControllerBase
{
    private readonly IMediator _mediator;

    public VolumesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpPut(ControllerRouteConstants.VolumeByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<VolumeResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateVolume(
        Guid volumeId,
        [FromBody] UpdateVolumeRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateVolumeCommand
        {
            VolumeId = volumeId,
            Title = request.Title,
            OrderIndex = request.OrderIndex
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<VolumeResponseDto>.Ok(result));
    }

    /// <summary>
    /// Reorder the chapters inside a volume. Owner only, or staff with
    /// <c>content.moderate</c>.
    /// </summary>
    [Authorize]
    [HttpPut(ControllerRouteConstants.VolumeChaptersOrderSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<ChapterSummaryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderVolumeChapters(
        Guid volumeId,
        [FromBody] ReorderChaptersRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ReorderChaptersCommand
        {
            VolumeId = volumeId,
            OrderedChapterIds = request.OrderedChapterIds
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<ChapterSummaryResponseDto>>.Ok(result));
    }
}
