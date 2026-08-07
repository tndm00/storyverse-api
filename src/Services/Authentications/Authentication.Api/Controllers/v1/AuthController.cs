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
}
