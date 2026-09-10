namespace Authentication.Api.Controllers.v1;

/// <summary>
/// Platform-admin user administration. Every action requires the
/// <c>users.manage</c> permission (held only by <c>PlatformAdmin</c>). Stays
/// thin: binds the request, sends a command through MediatR, wraps the result in
/// <see cref="ResponseDto{T}"/>.
/// </summary>
[ApiController]
[Route(ControllerRouteConstants.AdminUsersBase)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Grants a platform role to an account (e.g. <c>Moderator</c> so the account
    /// can work the reports queue). Idempotent. The grant reaches the affected
    /// account's JWT on its next sign-in.
    /// </summary>
    [HasPermission(StoryVersePermissions.Users.Manage)]
    [HttpPost(ControllerRouteConstants.AdminUserRolesSegment)]
    [ProducesResponseType(typeof(ResponseDto<UserRolesResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GrantRole(
        long userId,
        [FromBody] GrantUserRoleRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GrantUserRoleCommand { UserId = userId, Role = request.Role }, cancellationToken);

        return Ok(ResponseDto<UserRolesResponseDto>.Ok(result));
    }

    /// <summary>Revokes a platform role from an account. Idempotent. <c>Reader</c> cannot be revoked.</summary>
    [HasPermission(StoryVersePermissions.Users.Manage)]
    [HttpDelete(ControllerRouteConstants.AdminUserRoleByNameSegment)]
    [ProducesResponseType(typeof(ResponseDto<UserRolesResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RevokeRole(long userId, string role, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new RevokeUserRoleCommand { UserId = userId, Role = role }, cancellationToken);

        return Ok(ResponseDto<UserRolesResponseDto>.Ok(result));
    }
}
