namespace Library.Api.Controllers.v1;

/// <summary>
/// The reader's personal library ("tủ truyện"): deliberate saves of stories.
/// Every action is scoped to the authenticated caller; identity comes only from
/// the JWT, never the request body. Stays thin: bind, send through MediatR, wrap
/// in <see cref="ResponseDto{T}"/>.
/// </summary>
[ApiController]
[Authorize]
[Route(ControllerRouteConstants.LibraryBase)]
public sealed class LibraryController : ControllerBase
{
    private readonly IMediator _mediator;

    public LibraryController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>List the caller's library, newest first, optionally filtered by shelf.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<LibraryEntryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyLibrary(
        [FromQuery(Name = "shelf-status")] string shelfStatus,
        [FromQuery(Name = "page-number")] int pageNumber = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyLibraryQuery
        {
            ShelfStatus = shelfStatus,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<LibraryEntryResponseDto>>.Ok(result));
    }

    /// <summary>Add a story to the caller's library.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<LibraryEntryResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddEntry(
        [FromBody] AddLibraryEntryRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new AddLibraryEntryCommand
        {
            StoryId = request.StoryId,
            ShelfStatus = request.ShelfStatus
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<LibraryEntryResponseDto>.Ok(result));
    }

    /// <summary>Move a library entry to a different shelf.</summary>
    [HttpPut(ControllerRouteConstants.LibraryEntryShelfStatusSegment)]
    [ProducesResponseType(typeof(ResponseDto<LibraryEntryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeShelfStatus(
        Guid storyId,
        [FromBody] ChangeShelfStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeShelfStatusCommand
        {
            StoryId = storyId,
            ShelfStatus = request.ShelfStatus
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<LibraryEntryResponseDto>.Ok(result));
    }

    /// <summary>Remove a story from the caller's library.</summary>
    [HttpDelete(ControllerRouteConstants.LibraryEntryByStorySegment)]
    [ProducesResponseType(typeof(ResponseDto<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveEntry(Guid storyId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveLibraryEntryCommand { StoryId = storyId }, cancellationToken);

        return Ok(ResponseDto<object>.Ok(null));
    }
}
