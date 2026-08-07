namespace Be.StoryVerse.ApiCommon.Constants;

/// <summary>
/// Shared API/HTTP pipeline constants (headers, auth scheme, content type,
/// Swagger defaults) reused by every service host, per code-standard.md
/// section 11 (Constants Rules) and section 7.5 (ApiCommon Rules).
/// </summary>
public static class ApiCommonConstants
{
    public const string AuthorizationHeaderName = "Authorization";

    public const string BearerScheme = AuthConstants.BearerScheme;

    public const string JwtBearerFormat = "JWT";

    public const string BearerTokenDescription = "Enter a valid JWT access token: Bearer {token}";

    public const string JsonContentType = "application/json";

    public const string DefaultApiVersion = "v1";
}
