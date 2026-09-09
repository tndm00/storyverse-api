namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Verifies a Google-issued ID token (signature, issuer, audience, expiry) and
/// returns the identity it asserts. Implemented in Infrastructure against the
/// Google auth library so the Application layer stays free of that dependency.
/// </summary>
public interface IGoogleTokenValidator
{
    /// <summary>
    /// Validates <paramref name="idToken"/>. Throws
    /// <see cref="Be.StoryVerse.Core.Exceptions.BadRequestException"/> with a
    /// generic message when the token is missing, malformed, expired, or not
    /// issued for this application.
    /// </summary>
    Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// The subset of Google ID-token claims this service needs to provision or
/// resolve a <see cref="User"/>.
/// </summary>
public sealed record GoogleUserInfo(
    string Subject,
    string Email,
    bool EmailVerified,
    string Name,
    string PictureUrl);
