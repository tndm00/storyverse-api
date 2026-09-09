namespace Authentication.Application.Commands.GoogleLogin;

/// <summary>
/// Signs a user in with a Google ID token obtained by the frontend. Provisions a
/// password-less account on first use, or links Google to an existing account
/// that shares the verified email, then issues the same access + refresh token
/// pair as password login.
/// </summary>
public sealed class GoogleLoginCommand : ICommand<LoginResponseDto>
{
    public string IdToken { get; init; } = string.Empty;
}
