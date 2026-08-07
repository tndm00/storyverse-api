namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Payload bound from the API request body for <c>POST /v1/auth/register</c>.
/// </summary>
public sealed class RegisterRequestDto
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;
}
