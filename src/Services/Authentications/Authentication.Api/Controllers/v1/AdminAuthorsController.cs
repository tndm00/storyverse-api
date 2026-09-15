namespace Authentication.Api.Controllers.v1;

/// <summary>
/// Platform-admin author profile administration (CRUD over the author roster).
/// Every action requires the <c>users.manage</c> permission (held only by
/// <c>PlatformAdmin</c>). "Delete" is a soft suspend — stories already
/// published under a profile are unaffected. Stays thin: binds the request,
/// sends a command/query through MediatR, wraps the result in
/// <see cref="ResponseDto{T}"/>.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.AdminAuthorsBase)]
public sealed class AdminAuthorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAuthorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Paged author roster, optionally filtered by pen name keyword.</summary>
    [HasPermission(StoryVersePermissions.Users.Manage)]
    [HttpGet]
    [ProducesResponseType(typeof(ResponseDto<PagedResponseDto<AdminAuthorProfileResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthorProfiles(
        [FromQuery] string keyword,
        [FromQuery(Name = "page")] int page = 1,
        [FromQuery(Name = "page-size")] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAuthorProfilesAdminQuery { Keyword = keyword, PageNumber = page, PageSize = pageSize },
            cancellationToken);

        return Ok(ResponseDto<PagedResponseDto<AdminAuthorProfileResponseDto>>.Ok(result));
    }

    /// <summary>Creates a brand-new account and its author profile in one step.</summary>
    [HasPermission(StoryVersePermissions.Users.Manage)]
    [HttpPost]
    [ProducesResponseType(typeof(ResponseDto<AdminAuthorProfileResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAuthorProfile(
        [FromBody] CreateAuthorProfileAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAuthorProfileAdminCommand
        {
            Email = request.Email,
            Password = request.Password,
            DisplayName = request.DisplayName,
            PenName = request.PenName,
            Bio = request.Bio
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<AdminAuthorProfileResponseDto>.Ok(result));
    }

    /// <summary>Edits an existing author profile's fields.</summary>
    [HasPermission(StoryVersePermissions.Users.Manage)]
    [HttpPut(ControllerRouteConstants.AdminAuthorByIdSegment)]
    [ProducesResponseType(typeof(ResponseDto<AdminAuthorProfileResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAuthorProfile(
        long authorProfileId,
        [FromBody] UpdateAuthorProfileAdminRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAuthorProfileAdminCommand
        {
            AuthorProfileId = authorProfileId,
            PenName = request.PenName,
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl,
            BannerUrl = request.BannerUrl,
            Verified = request.Verified
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<AdminAuthorProfileResponseDto>.Ok(result));
    }

    /// <summary>Suspends (soft-delete) or reactivates an author profile.</summary>
    [HasPermission(StoryVersePermissions.Users.Manage)]
    [HttpPut(ControllerRouteConstants.AdminAuthorStatusSegment)]
    [ProducesResponseType(typeof(ResponseDto<AdminAuthorProfileResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SetAuthorProfileStatus(
        long authorProfileId,
        [FromBody] SetAuthorProfileStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new SetAuthorProfileStatusCommand
        {
            AuthorProfileId = authorProfileId,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<AdminAuthorProfileResponseDto>.Ok(result));
    }
}
