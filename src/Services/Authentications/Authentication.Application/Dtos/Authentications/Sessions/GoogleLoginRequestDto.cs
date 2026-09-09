namespace Authentication.Application.Dtos.Authentications.Sessions;

/// <summary>
/// Payload bound from the API request body for <c>POST /v1/auth/google</c>. The
/// <c>idToken</c> is the JWT the Google Identity Services client returns to the
/// frontend after the user picks an account.
/// </summary>
public sealed class GoogleLoginRequestDto
{
    public string IdToken { get; init; } = string.Empty;
}
