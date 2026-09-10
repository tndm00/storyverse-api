namespace Community.Api.Constants;

/// <summary>
/// General API-layer constants (Swagger metadata, host-level identifiers),
/// per code-standard.md section 11 (Constants Rules).
/// </summary>
public static class ApiConstants
{
    public const string SwaggerTitle = "Be.StoryVerse Community API";

    /// <summary>
    /// Header carrying the shared service-to-service secret on internal endpoints
    /// (comment moderation-visibility). Checked against <c>ServiceAuth:Token</c>.
    /// </summary>
    public const string ServiceTokenHeader = "X-Service-Token";
}
