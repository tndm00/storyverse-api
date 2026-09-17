namespace Authentication.Api.Controllers.v1;

/// <summary>
/// Registration, login, and current-user endpoints. Stays thin: binds the
/// request DTO, sends the command/query through MediatR, and wraps the result
/// in <see cref="ResponseDto{T}"/>, per api-guidelines.md section 8 and
/// codebase-architecture-flow.md section 4 (Request Flow).
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.AuthBase)]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>Initializes the controller with the MediatR sender used to dispatch commands/queries.</summary>
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a new login/auth identity. Public endpoint, per
    /// auth-guidelines.md section 14.
    /// </summary>
    [AllowAnonymous]
    [HttpPost(ControllerRouteConstants.RegisterSegment)]
    [ProducesResponseType(typeof(ResponseDto<RegisterResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            DisplayName = request.DisplayName
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<RegisterResponseDto>.Ok(result));
    }

    /// <summary>
    /// Authenticates a user and issues an access + refresh token. Public
    /// endpoint, per auth-guidelines.md section 14; brute-force protection is
    /// expected at the gateway/rate-limiting layer per section 11.
    /// </summary>
    [AllowAnonymous]
    [HttpPost(ControllerRouteConstants.LoginSegment)]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<LoginResponseDto>.Ok(result));
    }

    /// <summary>
    /// Exchanges a refresh token for a new access + refresh token pair. The new
    /// access token reflects the account's current roles and <c>author_id</c>
    /// (read from the database), so a client that just became an author can call
    /// this instead of a full re-login. The presented refresh token is rotated
    /// out; replaying it triggers reuse detection. Public endpoint.
    /// </summary>
    [AllowAnonymous]
    [HttpPost(ControllerRouteConstants.RefreshSegment)]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RefreshCommand { RefreshToken = request.RefreshToken }, cancellationToken);

        return Ok(ResponseDto<LoginResponseDto>.Ok(result));
    }

    /// <summary>
    /// Revokes the caller's current refresh token (sign-out). Idempotent. The
    /// access token remains valid until it expires — keep access-token TTL short.
    /// </summary>
    [Authorize]
    [HttpPost(ControllerRouteConstants.LogoutSegment)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequestDto request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new LogoutCommand { RefreshToken = request.RefreshToken }, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Signs a user in with a Google ID token from the frontend. Provisions a
    /// password-less account on first use. Public endpoint, per
    /// auth-guidelines.md section 14.
    /// </summary>
    [AllowAnonymous]
    [HttpPost(ControllerRouteConstants.GoogleLoginSegment)]
    [ProducesResponseType(typeof(ResponseDto<LoginResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GoogleLogin(
        [FromBody] GoogleLoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new GoogleLoginCommand { IdToken = request.IdToken };

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(ResponseDto<LoginResponseDto>.Ok(result));
    }

    /// <summary>
    /// Returns the authenticated caller's own profile. Identity is resolved
    /// from the validated JWT, never from a request parameter, per
    /// auth-guidelines.md section 3.
    /// </summary>
    [Authorize]
    [HttpGet(ControllerRouteConstants.MeSegment)]
    [ProducesResponseType(typeof(ResponseDto<CurrentUserResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);

        return Ok(ResponseDto<CurrentUserResponseDto>.Ok(result));
    }

    /// <summary>
    /// Turns the authenticated caller into an author by creating their
    /// publishing identity (Author Onboarding Flow step 2,
    /// product-workflow-context.md section 5.1). One profile per account; the
    /// <c>author_id</c> claim needed to publish is issued on the caller's next
    /// login (see <c>RequiresTokenRefresh</c> on the response).
    /// </summary>
    [Authorize]
    [HttpPost(ControllerRouteConstants.AuthorProfileSegment)]
    [ProducesResponseType(typeof(ResponseDto<AuthorProfileResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAuthorProfile(
        [FromBody] CreateAuthorProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAuthorProfileCommand
        {
            PenName = request.PenName,
            Bio = request.Bio,
            AvatarUrl = request.AvatarUrl,
            BannerUrl = request.BannerUrl
        };

        var result = await _mediator.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, ResponseDto<AuthorProfileResponseDto>.Ok(result));
    }

    /// <summary>
    /// Returns the authenticated caller's own author profile, or 404 when the
    /// account is reader-only. Identity is resolved from the JWT.
    /// </summary>
    [Authorize]
    [HttpGet(ControllerRouteConstants.AuthorProfileSegment)]
    [ProducesResponseType(typeof(ResponseDto<AuthorProfileResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyAuthorProfile(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyAuthorProfileQuery(), cancellationToken);

        return Ok(ResponseDto<AuthorProfileResponseDto>.Ok(result));
    }

    /// <summary>
    /// Public author profile by id (pen name, bio, verified flag). Anonymous —
    /// backs the reader-site <c>/author/:id</c> page.
    /// </summary>
    [AllowAnonymous]
    [HttpGet(ControllerRouteConstants.PublicAuthorSegment)]
    [ProducesResponseType(typeof(ResponseDto<PublicAuthorProfileResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicAuthorProfile(long authorProfileId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetPublicAuthorProfileQuery { AuthorProfileId = authorProfileId },
            cancellationToken);

        return Ok(ResponseDto<PublicAuthorProfileResponseDto>.Ok(result));
    }

    /// <summary>
    /// Internal, service-to-service: the owning user id for an AuthorProfile id.
    /// Not for end users. Authorized by a valid JWT OR the <c>X-Service-Token</c>
    /// header (<c>ServiceAuth:Token</c>). Used by the Content service to address
    /// chapter-review notifications to a story's author.
    /// </summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpGet(ControllerRouteConstants.InternalAuthorProfileLookupSegment)]
    [ProducesResponseType(typeof(ResponseDto<AuthorProfileLookupResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthorProfileLookup(long authorProfileId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAuthorProfileLookupQuery { AuthorProfileId = authorProfileId },
            cancellationToken);

        return Ok(ResponseDto<AuthorProfileLookupResponseDto>.Ok(result));
    }

    /// <summary>
    /// Internal, service-to-service: batch resolves account ids to public display
    /// names (<c>?ids=1,2,3</c>). Not for end users. Authorized by a valid JWT OR
    /// the <c>X-Service-Token</c> header. Used by Moderation (report reporter
    /// names) and Community (comment/rating author names). Unknown ids are omitted.
    /// </summary>
    [AllowAnonymous]
    [ServiceOrUserAuthorize]
    [HttpGet(ControllerRouteConstants.InternalUsersLookupSegment)]
    [ProducesResponseType(typeof(ResponseDto<IReadOnlyList<UserDirectoryEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersDirectory(
        [FromQuery(Name = "ids")] string ids,
        CancellationToken cancellationToken)
    {
        // Parse the comma-separated id list, discarding blanks and non-numeric/non-positive values.
        var parsed = (ids ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => long.TryParse(part, out var id) ? id : 0L)
            .Where(id => id > 0)
            .ToArray();

        var result = await _mediator.Send(
            new GetUsersDirectoryQuery { UserIds = parsed }, cancellationToken);

        return Ok(ResponseDto<IReadOnlyList<UserDirectoryEntryDto>>.Ok(result));
    }
}
