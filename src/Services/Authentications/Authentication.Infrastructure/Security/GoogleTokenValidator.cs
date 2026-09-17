namespace Authentication.Infrastructure.Security;

/// <summary>
/// Verifies Google ID tokens with <c>Google.Apis.Auth</c>, which checks the
/// signature against Google's published keys and the standard issuer/expiry, and
/// pins the audience to our configured OAuth client id.
/// </summary>
public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options;

    /// <summary>
    /// Creates the validator bound to the configured <see cref="GoogleAuthOptions"/>.
    /// </summary>
    public GoogleTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Verifies a Google ID token's signature, issuer, expiry and audience, and
    /// returns the caller's Google identity claims.
    /// </summary>
    public async Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new BadRequestException(ApplicationErrorConstants.GoogleAuthFailed);
        }

        // Pin verification to our configured OAuth client id.
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _options.ClientId }
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            // Library checks signature against Google's published keys plus issuer/expiry.
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException)
        {
            // Generic message: never reveal why verification failed.
            throw new BadRequestException(ApplicationErrorConstants.GoogleAuthFailed);
        }

        // Map Google's payload claims onto our own DTO shape.
        return new GoogleUserInfo(
            Subject: payload.Subject,
            Email: payload.Email,
            EmailVerified: payload.EmailVerified,
            Name: payload.Name,
            PictureUrl: payload.Picture);
    }
}
