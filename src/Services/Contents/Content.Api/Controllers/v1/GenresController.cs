namespace Content.Api.Controllers.v1;

/// <summary>
/// Genre taxonomy. Listing is public; create/update/hide require the
/// <c>genres.manage</c> permission (held by <c>PlatformAdmin</c>), enforced by
/// the shared permission policy.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.GenresBase)]
public sealed class GenresController : ControllerBase
{
    private readonly IMediator _mediator;

    public GenresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Lists genres. Inactive (hidden) genres are excluded unless requested.</summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<GenreResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGenres(
        [FromQuery(Name = "include-inactive")] bool includeInactive,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetGenresQuery { IncludeInactive = includeInactive },
            cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<GenreResponseDto>>.Ok(result));
    }

    /// <summary>Creates a new genre.</summary>
    [HasPermission(StoryVersePermissions.Genres.Manage)]
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<GenreResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateGenre([FromBody] CreateGenreRequestDto request, CancellationToken cancellationToken)
    {
        var command = new CreateGenreCommand
        {
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<GenreResponseDto>.Ok(result));
    }

    /// <summary>Updates a genre's name, description, display order, or active state.</summary>
    [HasPermission(StoryVersePermissions.Genres.Manage)]
    [HttpPut(ControllerRouteConstants.GenreBySlugSegment)]
    [ProducesResponseType(typeof(ResponseDto<GenreResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateGenre(
        string slug,
        [FromBody] UpdateGenreRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateGenreCommand
        {
            Slug = slug,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<GenreResponseDto>.Ok(result));
    }

    /// <summary>Toggles a genre's active/hidden state (soft-hide, no deletion).</summary>
    [HasPermission(StoryVersePermissions.Genres.Manage)]
    [HttpPost(ControllerRouteConstants.GenreHideSegment)]
    [ProducesResponseType(typeof(ResponseDto<GenreResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> HideGenre(string slug, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new HideGenreCommand { Slug = slug }, cancellationToken);

        return Ok(ResponseDto<GenreResponseDto>.Ok(result));
    }
}
