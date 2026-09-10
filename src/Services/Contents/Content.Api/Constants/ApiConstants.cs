namespace Content.Api.Constants;

/// <summary>
/// General API-layer constants (Swagger metadata, host-level identifiers),
/// per code-standard.md section 11 (Constants Rules).
/// </summary>
public static class ApiConstants
{
    public const string SwaggerTitle = "Be.StoryVerse Content API";

    /// <summary>
    /// Header carrying the shared service-to-service secret on internal endpoints
    /// (moderation-visibility, title lookup). Checked against <c>ServiceAuth:Token</c>.
    /// </summary>
    public const string ServiceTokenHeader = "X-Service-Token";
}
