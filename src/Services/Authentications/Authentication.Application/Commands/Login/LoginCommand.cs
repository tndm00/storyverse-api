namespace Authentication.Application.Commands.Login;

/// <summary>
/// Authenticates a user by email/password and issues an access + refresh token.
/// </summary>
public sealed class LoginCommand : ICommand<LoginResponseDto>
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
