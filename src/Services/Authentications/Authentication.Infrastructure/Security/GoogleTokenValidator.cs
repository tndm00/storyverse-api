namespace Authentication.Infrastructure.Security;

/// <summary>
/// Verifies Google ID tokens with <c>Google.Apis.Auth</c>, which checks the
/// signature against Google's published keys and the standard issuer/expiry, and
/// pins the audience to our configured OAuth client id.
/// </summary>
public sealed class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleAuthOptions _options;

    public GoogleTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleUserInfo> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw new BadRequestException(ApplicationErrorConstants.GoogleAuthFailed);
        }

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _options.ClientId }
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
        }
        catch (InvalidJwtException)
        {
            // Generic message: never reveal why verification failed.
            throw new BadRequestException(ApplicationErrorConstants.GoogleAuthFailed);
        }

        return new GoogleUserInfo(
            Subject: payload.Subject,
            Email: payload.Email,
            EmailVerified: payload.EmailVerified,
            Name: payload.Name,
            PictureUrl: payload.Picture);
    }
}
