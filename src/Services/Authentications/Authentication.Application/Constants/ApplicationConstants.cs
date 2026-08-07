namespace Authentication.Application.Constants;

public static class ApplicationConstants
{
    public const int AccessTokenDefaultMinutes = 15;
    public const int RefreshTokenDefaultDays = 14;
    public const int MinPasswordLength = 8;

    /// <summary>
    /// OAuth 2.0 bearer token type value returned in <c>LoginResponseDto.TokenType</c>.
    /// </summary>
    public const string BearerTokenType = AuthConstants.BearerScheme;
}
