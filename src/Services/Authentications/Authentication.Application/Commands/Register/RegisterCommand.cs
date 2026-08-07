namespace Authentication.Application.Commands.Register;

/// <summary>
/// Registers a new login/auth identity.
/// </summary>
public sealed class RegisterCommand : ICommand<RegisterResponseDto>
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;
}
