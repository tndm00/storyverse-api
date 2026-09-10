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
}
