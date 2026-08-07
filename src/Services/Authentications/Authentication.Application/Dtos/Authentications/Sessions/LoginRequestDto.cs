namespace Authentication.Application.Dtos.Authentications.Sessions;

/// <summary>
/// Payload bound from the API request body for <c>POST /v1/auth/login</c>.
/// </summary>
public sealed class LoginRequestDto
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
