namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Response returned after successful registration. Never includes the
/// password or password hash.
/// </summary>
public sealed class RegisterResponseDto
{
    public long UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;
}
