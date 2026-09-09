namespace Authentication.Application.Constants;

public static class ApplicationConstants
{
    public const int AccessTokenDefaultMinutes = 15;
    public const int RefreshTokenDefaultDays = 14;
    public const int MinPasswordLength = 8;

    /// <summary>Must match <c>AuthorProfileConfiguration</c>'s PenName max length.</summary>
    public const int MaxPenNameLength = 100;

    /// <summary>Must match <c>AuthorProfileConfiguration</c>'s Bio max length.</summary>
    public const int MaxBioLength = 2000;

    /// <summary>
    /// OAuth 2.0 bearer token type value returned in <c>LoginResponseDto.TokenType</c>.
    /// </summary>
    public const string BearerTokenType = AuthConstants.BearerScheme;

    /// <summary>
    /// Value stored in <c>User.ExternalProvider</c> for Google-linked accounts.
    /// </summary>
    public const string GoogleProvider = "Google";
}
